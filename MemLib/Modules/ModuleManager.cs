using System;
using System.Collections.Generic;
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
            var native = NativeModules.FirstOrDefault(m => m.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            return native == null ? null : new RemoteModule(m_Process, native);
        }

        #region Inject

        public InjectedModule Inject(string module, bool mustBeDisposed = true) {
            if (!Path.HasExtension(module))
                module += ".dll";
            var mod = InternalInject(module, mustBeDisposed);
            if (mod != null && !m_InjectedModules.Contains(mod))
                m_InjectedModules.Add(mod);
            return mod;
        }

        #endregion

        #region Eject

        public void Eject(RemoteModule module) {
            if (module == null || !module.IsValid) return;

            var injected = m_InjectedModules.FirstOrDefault(m => m.Equals(module));
            if (injected != null)
                m_InjectedModules.Remove(injected);

            InternalEject(module.BaseAddress);
        }

        public void Eject(string module) {
            if (!Path.HasExtension(module))
                module += ".dll";
            Eject(RemoteModules.FirstOrDefault(m => m.Name.Equals(module, StringComparison.OrdinalIgnoreCase) 
                                                        || m.Path.Equals(module, StringComparison.OrdinalIgnoreCase)));
        }

        public void Eject(IntPtr module) {
            Eject(RemoteModules.FirstOrDefault(m => m.BaseAddress == module));
        }

        #endregion

        #region Internals

        private IEnumerable<NativeModule> InternalEnumProcessModules() {
            var flags = m_Process.Is64Bit ? ListModulesFlags.ListModules64Bit : ListModulesFlags.ListModules32Bit;
            if (!ModuleHelper.EnumProcessModules(m_Process.Handle, out var modHandles, flags))
                return Enumerable.Empty<NativeModule>();
            return modHandles.Select(h => new NativeModule(m_Process.Handle, h));
        }

        private InjectedModule InternalInject(string module, bool mustBeDisposed) {
            var thread = m_Process.Threads.CreateAndJoin(this["kernel32"]["LoadLibraryA"].BaseAddress, module);
            if (thread.GetExitCode<IntPtr>() == IntPtr.Zero) return null;
            var native = NativeModules.FirstOrDefault(m => m.ModuleName.Equals(Path.GetFileName(module), StringComparison.OrdinalIgnoreCase));
            return native == null ? null : new InjectedModule(m_Process, native, mustBeDisposed);
        }

        private void InternalEject(IntPtr hModule) {
            m_Process.Threads.CreateAndJoin(this["kernel32"]["FreeLibrary"].BaseAddress, hModule);
        }

        #endregion
        
        #region IDisposable

        void IDisposable.Dispose() {
            foreach (var module in m_InjectedModules.Where(m => m.MustBeDisposed).ToList()) {
                module.Dispose();
            }
            RemoteModule.ExportCache.RemoveWhere(export => export.ProcessHandle == m_Process.UnsafeHandle);
            GC.SuppressFinalize(this);
        }

        ~ModuleManager() {
            ((IDisposable) this).Dispose();
        }

        #endregion
    }

}