﻿using System;
using MemLib.Memory;

namespace MemLib.Internals {
    internal interface IMarshalledValue : IDisposable {
        RemoteAllocation Allocated { get; }
        IntPtr Reference { get; }
        Type Type { get; }
        bool IsByRef { get; }
    }
}