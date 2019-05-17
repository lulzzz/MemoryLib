using System;

namespace MemLib.Modules {
    internal class CachedModuleExport : IEquatable<CachedModuleExport> {
        public IntPtr ProcessHandle { get; }
        public IntPtr ModuleHandle { get; }
        public string Name { get; }
        public RemoteFunction Function { get; set; }

        public CachedModuleExport(IntPtr hProcess, IntPtr hModule, string name) {
            ProcessHandle = hProcess;
            ModuleHandle = hModule;
            Name = name;
        }

        #region Equality members

        public bool Equals(CachedModuleExport other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ProcessHandle.Equals(other.ProcessHandle) 
                   && ModuleHandle.Equals(other.ModuleHandle) 
                   && Name.Equals(other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CachedModuleExport) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = ProcessHandle.GetHashCode();
                hashCode = (hashCode * 397) ^ ModuleHandle.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(CachedModuleExport left, CachedModuleExport right) {
            return Equals(left, right);
        }

        public static bool operator !=(CachedModuleExport left, CachedModuleExport right) {
            return !Equals(left, right);
        }

        #endregion
    }
}