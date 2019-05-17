using System;
using System.Collections.Generic;
using System.Text;
using MemLib.Memory;
using MemLib.PeHeader.Structures;

namespace MemLib.PeHeader {
    public class PeHeaderParser : RemotePointer {
        public bool Is64Bit => Read<ushort>((int)DosHeader.e_lfanew + 0x04) == 0x8664;
        public bool IsPeFile => DosHeader.e_magic == 0x4D5A;
        public IEnumerable<ExportFunction> ExportFunctions => ReadExports();
        public ImageDosHeader DosHeader { get; }

        private ImageNtHeader m_NtHeader;
        public ImageNtHeader NtHeader => m_NtHeader ?? (m_NtHeader = new ImageNtHeader(m_Process, BaseAddress + (int) DosHeader.e_lfanew, Is64Bit));

        private ImageExportDirectory m_ExportDirectory;
        public ImageExportDirectory ExportDirectory {
            get {
                if (m_ExportDirectory != null) return m_ExportDirectory;
                var exportVa = NtHeader.OptionalHeader.DataDirectory[0].VirtualAddress;
                if (exportVa != 0)
                    m_ExportDirectory = new ImageExportDirectory(m_Process, BaseAddress + (int) exportVa);
                return m_ExportDirectory;
            }
        }

        private ImageSectionHeaders m_SectionHeaders;
        public ImageSectionHeaders SectionHeaders {
            get {
                if (m_SectionHeaders != null) return m_SectionHeaders;
                var numSections = (int)NtHeader.FileHeader.NumberOfSections;
                var secHeaderStart = DosHeader.e_lfanew + NtHeader.FileHeader.SizeOfOptionalHeader + 0x18;
                return m_SectionHeaders = new ImageSectionHeaders(m_Process, BaseAddress + (int)secHeaderStart, numSections);
            }
        }

        public PeHeaderParser(RemoteProcess process, IntPtr moduleBase) : base(process, moduleBase) {
            DosHeader = new ImageDosHeader(process, moduleBase);
        }
        
        private IEnumerable<ExportFunction> ReadExports() {
            if(ExportDirectory == null || ExportDirectory.NumberOfFunctions == 0)
                yield break;
            var expFuncs = new ExportFunction[ExportDirectory.NumberOfFunctions];

            var funcOffset = (int)ExportDirectory.AddressOfFunctions;
            var ordOffset = (int)ExportDirectory.AddressOfNameOrdinals;
            var nameOffset = (int)ExportDirectory.AddressOfNames;
            var exportBase = (int)ExportDirectory.Base;
            var numberOfNames = (int)ExportDirectory.NumberOfNames;

            for (var i = 0; i < expFuncs.Length; i++) {
                var ordinal = i + exportBase;
                var address = Read<int>(funcOffset + sizeof(uint) * i);
                expFuncs[i] = new ExportFunction(string.Empty, address, ordinal);
            }
            for (var i = 0; i < numberOfNames; i++) {
                var ordinalIndex = Read<ushort>(ordOffset + sizeof(ushort) * i);
                var tmp = expFuncs[ordinalIndex];
                if(tmp.RelativeAddress <= 0) continue;
                var nameAddr = Read<int>(nameOffset + sizeof(uint) * i);
                var name = nameAddr == 0 ? string.Empty : ReadString(nameAddr, Encoding.UTF8);
                
                expFuncs[ordinalIndex] = new ExportFunction(name, tmp.RelativeAddress, tmp.Ordinal);
                if (expFuncs[ordinalIndex].RelativeAddress != 0)
                    yield return expFuncs[ordinalIndex];
            }
        }

    }
}