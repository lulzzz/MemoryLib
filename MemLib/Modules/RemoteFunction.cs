using System;
using MemLib.Memory;

namespace MemLib.Modules {
    public class RemoteFunction : RemotePointer {
        public string Name { get; }
        public bool IsMangled => !Name.Equals(UndecoratedName, StringComparison.Ordinal);
        public string UndecoratedName => ModuleHelper.UnDecorateSymbolName(Name);

        public RemoteFunction(RemoteProcess process, IntPtr address, string name) : base(process, address) {
            Name = name;
        }

        public override string ToString() => $"Address = 0x{BaseAddress.ToInt64():X8} Name = {Name}";
    }
}