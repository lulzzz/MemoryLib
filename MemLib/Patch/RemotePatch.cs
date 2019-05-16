using System;
using System.Linq;

namespace MemLib.Patch {
    public sealed class RemotePatch : IDisposable, IEquatable<RemotePatch> {
        private readonly RemoteProcess m_Process;
        public bool IsDisposed { get; private set; }
        public bool MustBeDisposed { get; set; }

        public byte[] NewBytes { get; private set; }
        public byte[] OldBytes { get; private set; }

        public int Size => NewBytes.Length;
        public IntPtr Address { get; }
        public string Name { get; }

        public bool IsApplied {
            get {
                var remote = m_Process.Read<byte>(Address, Size);
                return remote.SequenceEqual(NewBytes);
            }
        }

        internal RemotePatch(RemoteProcess process, string name, IntPtr address, byte[] newBytes, bool mustDispose) {
            m_Process = process;
            Name = name;
            NewBytes = newBytes;
            Address = address;
            MustBeDisposed = mustDispose;
        }

        public void Apply() {
            if (IsApplied) return;
            OldBytes = m_Process.Read<byte>(Address, NewBytes.Length);
            m_Process.Write(Address, NewBytes);
        }

        public void Remove() {
            m_Process.Patch.RemovePatch(this);
        }

        internal void InternalRemove() {
            if (!IsApplied) return;
            m_Process.Write(Address, OldBytes);
        }

        public void ChangePatchBytes(byte[] newBytes) {
            if (IsApplied) {
                m_Process.Write(Address, OldBytes);
                OldBytes = m_Process.Read<byte>(Address, newBytes.Length);
                m_Process.Write(Address, newBytes);
            } else {
                OldBytes = m_Process.Read<byte>(Address, newBytes.Length);
            }
            NewBytes = newBytes;
        }

        #region IDisposable

        public void Dispose() {
            if (IsDisposed) return;
            IsDisposed = true;
            if (m_Process.IsRunning)
                Remove();
            GC.SuppressFinalize(this);
        }

        ~RemotePatch() {
            if (MustBeDisposed)
                Dispose();
        }

        #endregion

        #region IEquatable

        public bool Equals(RemotePatch other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_Process.Equals(other.m_Process) &&
                   Address.Equals(other.Address) &&
                   string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RemotePatch patch && Equals(patch);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = m_Process.GetHashCode();
                hashCode = (hashCode * 397) ^ Address.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RemotePatch left, RemotePatch right) {
            return Equals(left, right);
        }

        public static bool operator !=(RemotePatch left, RemotePatch right) {
            return !Equals(left, right);
        }

        #endregion
    }
}