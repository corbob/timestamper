using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace TimeStamper
{
    internal sealed class PseudoConsole : IDisposable
    {
        private IntPtr _handle;

        private PseudoConsole(IntPtr handle) => _handle = handle;

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, short width, short height)
        {
            var coord = new NativeMethods.COORD { X = width, Y = height };
            int hr = NativeMethods.CreatePseudoConsole(coord, inputReadSide, outputWriteSide, 0, out IntPtr hPC);
            if (hr != 0)
                throw new InvalidOperationException("Failed to create pseudo console.", Marshal.GetExceptionForHR(hr));
            return new PseudoConsole(hPC);
        }

        internal IntPtr Handle => _handle;

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ClosePseudoConsole(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }
}