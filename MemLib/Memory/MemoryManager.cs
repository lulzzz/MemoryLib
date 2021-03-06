﻿using System;
using System.Collections.Generic;
using System.Linq;
using MemLib.Native;

namespace MemLib.Memory {
    public sealed class MemoryManager : IDisposable {
        private readonly RemoteProcess m_Process;
        private readonly HashSet<RemoteAllocation> m_RemoteAllocations = new HashSet<RemoteAllocation>();
        private readonly IntPtr m_RegionLowerLimit;
        private readonly IntPtr m_RegionUpperLimit;

        public List<RemoteAllocation> RemoteAllocations => m_RemoteAllocations.ToList();
        public IEnumerable<RemoteRegion> Regions => QueryRegions(m_RegionLowerLimit, m_RegionUpperLimit);

        internal MemoryManager(RemoteProcess process) {
            m_Process = process;
            //TODO: Use better values for limits
            if (process.Native.MainModule != null)
                m_RegionLowerLimit = process.Native.MainModule.BaseAddress;
            m_RegionUpperLimit = process.Is64Bit ? new IntPtr(0x7fff_ffffffff) : new IntPtr(0x7fffffff);
        }

        private IEnumerable<RemoteRegion> QueryRegions(IntPtr start, IntPtr end) {
            return MemoryHelper.Query(m_Process.Handle, start, end).Select(region => new RemoteRegion(m_Process, region.BaseAddress));
        }

        #region Allocate

        public RemoteAllocation Allocate(int size, bool mustBeDisposed = true) {
            return Allocate(size, MemoryProtectionFlags.ExecuteReadWrite, mustBeDisposed);
        }

        public RemoteAllocation Allocate(int size, MemoryProtectionFlags protection, bool mustBeDisposed = true) {
            var memory = new RemoteAllocation(m_Process, size, protection, mustBeDisposed);
            m_RemoteAllocations.Add(memory);
            return memory;
        }

        #endregion

        #region Deallocate

        public void Deallocate(RemoteAllocation allocation) {
            if (m_RemoteAllocations.Contains(allocation))
                m_RemoteAllocations.Remove(allocation);
            if (!allocation.IsDisposed)
                allocation.Dispose();
        }

        public void Deallocate(IntPtr baseAddress) {
            var alloc = m_RemoteAllocations.FirstOrDefault(a => a.BaseAddress == baseAddress);
            if(alloc == null)
                MemoryHelper.Free(m_Process.Handle, baseAddress);
            else Deallocate(alloc); 
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose() {
            foreach (var allocation in m_RemoteAllocations.Where(r => r.MustBeDisposed).ToList()) {
                allocation.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~MemoryManager() {
            ((IDisposable) this).Dispose();
        }

        #endregion
    }
}