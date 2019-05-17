using System;
using System.Collections.Generic;
using System.Linq;
using MemLib.Memory;
using MemLib.PeHeader;

namespace MemLib.Modules {
    public class RemoteModule : RemoteRegion {
        internal static readonly HashSet<CachedModuleExport> ExportCache = new HashSet<CachedModuleExport>();

        public NativeModule Native { get; }
        public PeHeaderParser PeHeader { get; }
        public string Name => Native.ModuleName;
        public string Path => Native.FileName;
        public long Size => Native.ModuleMemorySize;
        public bool IsMainModule => m_Process.Native.MainModule != null && m_Process.Native.MainModule.BaseAddress == BaseAddress;
        public override bool IsValid => base.IsValid && m_Process.Modules.NativeModules.Any(m => m.BaseAddress == BaseAddress && m.ModuleName == Name);

        public IEnumerable<RemoteFunction> Exports => PeHeader.ExportFunctions.Select(f => new RemoteFunction(m_Process, BaseAddress + f.RelativeAddress, f.Name));

        public RemoteFunction this[string functionName] => FindFunction(functionName);

        internal RemoteModule(RemoteProcess process, NativeModule module) : base(process, module.BaseAddress) {
            Native = module;
            PeHeader = new PeHeaderParser(process, module.BaseAddress);
        }

        public void Eject() {
            m_Process.Modules.Eject(this);
            BaseAddress = IntPtr.Zero;
        }

        private RemoteFunction FindFunction(string name) {
            var cache = new CachedModuleExport(m_Process.UnsafeHandle, BaseAddress, name);
            if (ExportCache.TryGetValue(cache, out var cacheVal))
                return cacheVal.Function;

            var func = Exports.FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.Ordinal) ||
                f.UndecoratedName.Equals(name, StringComparison.Ordinal));
            if (func == null) return null;
            cache.Function = func;
            ExportCache.Add(cache);
            return func;
        }
        
        public override string ToString() => $"BaseAddress=0x{BaseAddress.ToInt64():X} Size=0x{Size:X} Name={Name}";
    }

}