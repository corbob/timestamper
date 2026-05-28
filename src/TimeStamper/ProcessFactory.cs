using System;
using System.Runtime.InteropServices;

namespace TimeStamper
{
    internal static class ProcessFactory
    {
        internal static NativeMethods.PROCESS_INFORMATION Start(string processName, string arguments, PseudoConsole pseudoConsole)
        {
            var startupInfoEx = CreateStartupInfoEx(pseudoConsole);
            var commandLine = $"\"{processName}\" {arguments}";

            bool success = NativeMethods.CreateProcess(
                null,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                NativeMethods.EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref startupInfoEx,
                out var processInfo);

            NativeMethods.DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
            Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);

            if (!success)
                throw new InvalidOperationException($"Failed to create process. Error: {Marshal.GetLastWin32Error()}");

            NativeMethods.CloseHandle(processInfo.hThread);
            return processInfo;
        }

        private static NativeMethods.STARTUPINFOEX CreateStartupInfoEx(PseudoConsole pseudoConsole)
        {
            var startupInfoEx = new NativeMethods.STARTUPINFOEX();
            startupInfoEx.StartupInfo.cb = Marshal.SizeOf<NativeMethods.STARTUPINFOEX>();

            var lpSize = IntPtr.Zero;
            NativeMethods.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            startupInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);

            if (!NativeMethods.InitializeProcThreadAttributeList(startupInfoEx.lpAttributeList, 1, 0, ref lpSize))
            {
                Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
                throw new InvalidOperationException($"Failed to initialize proc thread attribute list. Error: {Marshal.GetLastWin32Error()}");
            }

            if (!NativeMethods.UpdateProcThreadAttribute(
                startupInfoEx.lpAttributeList,
                0,
                (IntPtr)NativeMethods.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                pseudoConsole.Handle,
                (IntPtr)IntPtr.Size,
                IntPtr.Zero,
                IntPtr.Zero))
            {
                NativeMethods.DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
                Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
                throw new InvalidOperationException($"Failed to update proc thread attribute. Error: {Marshal.GetLastWin32Error()}");
            }

            return startupInfoEx;
        }
    }
}