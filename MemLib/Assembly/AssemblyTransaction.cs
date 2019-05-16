using System;
using System.Collections.Generic;
using System.Text;
using MemLib.Internals;

namespace MemLib.Assembly {
    public sealed class AssemblyTransaction : IDisposable {
        private readonly RemoteProcess m_Process;
        private readonly StringBuilder m_Code;
        private IntPtr m_ExitCode;

        public int CodeOffset { get; set; }
        public IntPtr BaseAddress { get; private set; }
        public bool IsAutoExecuted { get; set; }

        public AssemblyTransaction(RemoteProcess process, bool autoExecute) : this(process, IntPtr.Zero, autoExecute){}

        public AssemblyTransaction(RemoteProcess process, int codeOffset, bool autoExecute) : this(process, IntPtr.Zero, autoExecute) {
            CodeOffset = codeOffset;
        }

        public AssemblyTransaction(RemoteProcess process, IntPtr address, int codeOffset, bool autoExecute) : this(process, address, autoExecute) {
            CodeOffset = codeOffset;
        }

        public AssemblyTransaction(RemoteProcess process, IntPtr address, bool autoExecute) {
            m_Process = process;
            m_Code = new StringBuilder();
            IsAutoExecuted = autoExecute;
            BaseAddress = address;
        }

        public override string ToString() => m_Code.ToString();

        #region GetExitCode

        public T GetExitCode<T>() {
            return MarshalType<T>.PtrToObject(m_Process, m_ExitCode);
        }

        #endregion

        #region Assemble

        public bool Assemble(out byte[] data) {
            if (m_Code.Length > 0) {
                return m_Process.Assembly.Assembler.Assemble(m_Code.ToString(), BaseAddress, out data);
            }
            data = null;
            return false;
        }

        public byte[] Assemble() {
            return Assemble(out var data) ? data : null;
        }

        #endregion

        #region Clear

        public void Clear() {
            m_Code.Clear();
        }

        #endregion

        #region Add

        public void Add(string asm, params object[] args) {
            m_Code.AppendLine(args.Length == 0 ? asm : string.Format(asm, args));
        }

        public void Add(IEnumerable<string> asmStrings) {
            if(asmStrings == null) return;
            foreach (var asmString in asmStrings) {
                m_Code.AppendLine(asmString);
            }
        }

        #endregion

        #region Prepend

        public void Prepend(string asm, params object[] args) {
            if (!asm.EndsWith(Environment.NewLine)) asm += Environment.NewLine;
            m_Code.Insert(0, string.Format(asm, args));
        }

        public void Prepend(IEnumerable<string> asmStrings) {
            Insert(0, asmStrings);
        }

        #endregion

        #region Insert

        public void Insert(int index, string asm, params object[] args) {
            m_Code.Insert(index, string.Format(asm, args));
        }

        public void Insert(int index, IEnumerable<string> asmStrings) {
            if (asmStrings == null) return;
            var sb = new StringBuilder();
            foreach (var asmString in asmStrings) {
                sb.AppendLine(asmString);
            }
            m_Code.Insert(index, sb.ToString());
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            if (BaseAddress != IntPtr.Zero) {
                if (IsAutoExecuted) {
                    m_Process.Assembly.Inject(m_Code.ToString(), BaseAddress);
                    m_ExitCode = m_Process.Assembly.Execute(BaseAddress + CodeOffset);
                } else {
                    m_Process.Assembly.Inject(m_Code.ToString(), BaseAddress);
                }
            }
            if (BaseAddress == IntPtr.Zero && IsAutoExecuted) {
                using (var alloc = m_Process.Assembly.Inject(m_Code.ToString())) {
                    BaseAddress = alloc.BaseAddress;
                    m_ExitCode = m_Process.Assembly.Execute(alloc.BaseAddress);
                }
            }
        }

        #endregion
    }
}