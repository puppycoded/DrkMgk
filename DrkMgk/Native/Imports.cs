using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DrkMgk
{
    internal unsafe class Imports
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeMemoryHandle OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetProcessId(
            SafeMemoryHandle handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process(
            SafeMemoryHandle hProcess,
            [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetClassName(
            SafeMemoryHandle hWnd,
            StringBuilder lpClassName,
            int nMaxCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadProcessMemory(
            SafeMemoryHandle hProcess,
            IntPtr dwAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WriteProcessMemory(
            SafeMemoryHandle hProcess,
            IntPtr dwAddress,
            [In] byte[] lpBuffer,
            int dwSize,
            out int iBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
        internal static extern void MoveMemory(
            void* destination,
            void* source,
            int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(
            [Optional] IntPtr lpAddress,
            int dwSize,
            MemoryAllocationState dwAllocationType,
            MemoryProtectionType dwProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(
            SafeMemoryHandle hProcess,
            [Optional] IntPtr lpAddress,
            int dwSize,
            MemoryAllocationState dwAllocationType,
            MemoryProtectionType dwProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFree(
            IntPtr lpAddress,
            int dwSize,
            MemoryFreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFreeEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            int dwSize,
            MemoryFreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(
            IntPtr lpAddress,
            int dwSize,
            MemoryProtectionType flNewProtect,
            out MemoryProtectionType lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtectEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            int dwSize,
            MemoryProtectionType flNewProtect,
            out MemoryProtectionType lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualQuery(
            IntPtr lpAddress,
            out MemoryBasicInformation lpBuffer,
            int dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualQueryEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            out MemoryBasicInformation lpBuffer,
            int dwLength);
    }
}
