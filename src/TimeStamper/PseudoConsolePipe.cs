using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace TimeStamper
{
    internal sealed class PseudoConsolePipe : IDisposable
    {
        public readonly SafeFileHandle ReadSide;
        public readonly SafeFileHandle WriteSide;

        public PseudoConsolePipe()
        {
            if (!NativeMethods.CreatePipe(out ReadSide, out WriteSide, IntPtr.Zero, 0))
                throw new InvalidOperationException("Failed to create pipe.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }

        public void Dispose()
        {
            ReadSide?.Dispose();
            WriteSide?.Dispose();
        }
    }
}