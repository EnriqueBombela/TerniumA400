﻿using System;
using System.Runtime.InteropServices;

namespace AAM_Lepton.Interfaces
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);

        //void GetBuffer(out Int16* buffer, out uint capacity);
    }
}
