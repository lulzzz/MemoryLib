using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MemLib.Native;

namespace MemLib.Modules {
    public sealed class ModuleManager : IDisposable {
        private readonly RemoteProcess m_Process;
        private readonly HashSet<InjectedModule> m_InjectedModules = new HashSet<InjectedModule>();
        internal IEnumerable<NativeModule> NativeModules => InternalEnumProcessModules();

        private RemoteModule m_MainModule;
        public RemoteModule MainModule => m_MainModule ?? (m_MainModule = FetchModule(m_Process.Native.MainModule?.ModuleName));
        public IEnumerable<RemoteModule> RemoteModules => NativeModules.Select(m => new RemoteModule(m_Process, m));

        public RemoteModule this[string moduleName] => FetchModule(moduleName);

        internal ModuleManager(RemoteProcess process) {
            m_Process = process;
        }

        private RemoteModule FetchModule(string moduleName) {
            if (!Path.HasExtension(moduleName))
                moduleName += ".dll";
            var nativeMod = NativeModules.FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            return nativeMod == null ? null : new RemoteModule(m_Process, nativeMod);
        }
        
        [DebuggerStepThrough]
        public InjectedModule Inject(string moduleFile, bool mustBeDisposed = true) {
            var module = InternalInject(moduleFile, mustBeDisposed);
            if (module != null && !m_InjectedModules.Contains(module)) 
                m_InjectedModules.Add(module);
            return module;
        }

        public void Eject(RemoteModule module) {
            if (module == null || !module.IsValid) return;
            
            var injected = m_InjectedModules.FirstOrDefault(m => m.Equals(module));
            if (injected != null)
                m_InjectedModules.Remove(injected);

            InternalEject(module);
        }

        public void Eject(string moduleName) {
            var module = RemoteModules.FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            if (module != null)
                InternalEject(module);
        }

        private IEnumerable<NativeModule> InternalEnumProcessModules() {
            var flags = m_Process.Is64Bit ? ListModulesFlags.ListModules64Bit : ListModulesFlags.ListModules32Bit;
            if (!ModuleHelper.EnumProcessModules(m_Process.Handle, out var modHandles, flags))
                yield break;
            foreach (var handle in modHandles)
                yield return new NativeModule(m_Process.Handle, handle);
        }

        private InjectedModule InternalInject(string path, bool mustBeDisposed) {
            var thread = m_Process.Threads.CreateAndJoin(m_Process["kernel32"]["LoadLibraryA"].BaseAddress, path);
            if (thread.GetExitCode<IntPtr>() == IntPtr.Zero) return null;
            var moduleName = Path.GetFileName(path);
            var nativeMod = m_Process.Modules.NativeModules.First(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            return new InjectedModule(m_Process, nativeMod, mustBeDisposed);
        }

        private void InternalEject(RemoteModule module) {
            m_Process.Threads.CreateAndJoin(m_Process["kernel32"]["FreeLibrary"].BaseAddress, module.BaseAddress);
        }

        #region IDisposable

        void IDisposable.Dispose() {
            foreach (var module in m_InjectedModules.Where(m => m.MustBeDisposed).ToList()) {
                module.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~ModuleManager() {
            ((IDisposable) this).Dispose();
        }

        #endregion
    }

}