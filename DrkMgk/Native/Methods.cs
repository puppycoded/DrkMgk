using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace DrkMgk
{
    public static unsafe class Native
    {
        public static SafeMemoryHandle OpenProcess(int pId,
            ProcessAccessFlags accessRights = ProcessAccessFlags.PROCESS_ALL_ACCESS)
        {
            SafeMemoryHandle processHandle = Imports.OpenProcess(accessRights, false, pId);
            if (processHandle == null || processHandle.IsInvalid || processHandle.IsClosed)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to open process {pId} with access {accessRights.ToString("X")}");
            return processHandle;
        }

        public static int GetProcessId(SafeMemoryHandle processHandle)
        {
            int pId = Imports.GetProcessId(processHandle);
            if (pId == 0)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to get Id from process handle 0x{processHandle.DangerousGetHandle().ToString("X")}");
            return pId;
        }

        public static bool Is64BitProcess(SafeMemoryHandle processHandle)
        {
            bool Is64BitProcess;
            if (!Imports.IsWow64Process(processHandle, out Is64BitProcess))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to determine if process handle 0x{processHandle.DangerousGetHandle().ToString("X")} is 64 bit");
            return !Is64BitProcess;
        }

        public static string GetClassName(SafeMemoryHandle windowHandle)
        {
            StringBuilder className = new StringBuilder(char.MaxValue);
            if (Imports.GetClassName(windowHandle, className, className.Capacity) == 0)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to get class name from window handle 0x{windowHandle.DangerousGetHandle().ToString("X")}");
            return className.ToString();
        }

        public static bool CloseHandle(IntPtr handle)
        {
            if (!Imports.CloseHandle(handle))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to close handle 0x{handle.ToString("X")}");
            return true;
        }

        public static int ReadProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, int size)
        {
            int bytesRead;
            if (!Imports.ReadProcessMemory(processHandle, address, buffer, size, out bytesRead))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to read memory from 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return bytesRead;
        }

        public static int WriteProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [In] byte[] buffer, int size)
        {
            int bytesWritten;
            if (!Imports.WriteProcessMemory(processHandle, address, buffer, size, out bytesWritten))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to write memory at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return bytesWritten;
        }

        public static IntPtr Alloc([Optional] IntPtr address, int size,
            MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            IntPtr memAddress = Imports.VirtualAlloc(address, size, MemoryAllocationState.MEM_COMMIT, protect);
            if (memAddress == IntPtr.Zero)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to allocate memory at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return memAddress;
        }

        public static IntPtr Alloc(SafeMemoryHandle processHandle, [Optional] IntPtr address, int size,
            MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            IntPtr memAddress = Imports.VirtualAllocEx(processHandle, address, size, MemoryAllocationState.MEM_COMMIT, protect);
            if (memAddress == IntPtr.Zero)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to allocate memory to process handle " +
                    $"0x{processHandle.DangerousGetHandle().ToString("X")} at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return memAddress;
        }

        public static bool Free(IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
        {
            if (!Imports.VirtualFree(address, size, free))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to free memory at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return true;
        }

        public static bool Free(SafeMemoryHandle processHandle, IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
        {
            if (!Imports.VirtualFreeEx(processHandle, address, size, free))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to free memory from process handle " +
                    $"0x{processHandle.DangerousGetHandle().ToString("X")} at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return true;
        }

        public static void Copy(void* destination, void* source, int size)
        {
            try
            {
                Imports.MoveMemory(destination, source, size);
            }
            catch
            {
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to copy memory to {(*(ulong*)(destination)).ToString($"X{IntPtr.Size}")} " +
                    $"from {(*(ulong*)(source)).ToString($"X{IntPtr.Size}")}[Size: {size}]");
            }
        }

        public static void Copy(IntPtr destination, IntPtr source, int size)
        {
            Copy(destination.ToPointer(), source.ToPointer(), size);
        }

        public static MemoryProtectionType ChangeMemoryProtection(IntPtr address, int size,
            MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            MemoryProtectionType oldProtect;
            if (!Imports.VirtualProtect(address, size, newProtect, out oldProtect))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to change memory protection at " +
                    $"0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}] to {newProtect.ToString("X")}");
            return oldProtect;
        }

        public static MemoryProtectionType ChangeMemoryProtection(SafeMemoryHandle processHandle, IntPtr address, int size,
            MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            MemoryProtectionType oldProtect;
            if (!Imports.VirtualProtectEx(processHandle, address, size, newProtect, out oldProtect))
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to change memory protection of process handle " +
                    $"0x{processHandle.DangerousGetHandle().ToString("X")} at 0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}] to {newProtect.ToString("X")}");
            return oldProtect;
        }

        public static MemoryBasicInformation Query(IntPtr address, int size)
        {
            MemoryBasicInformation memInfo;
            if (Imports.VirtualQuery(address, out memInfo, size) == 0)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to retrieve memory information from " +
                    $"0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return memInfo;
        }

        public static MemoryBasicInformation Query(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            MemoryBasicInformation memInfo;
            if (Imports.VirtualQueryEx(processHandle, address, out memInfo, size) == 0)
                throw new Win32Exception($"[Win32 Error: {Marshal.GetLastWin32Error()}] " +
                    $"Unable to retrieve memory information with " +
                    $"0x{processHandle.DangerousGetHandle().ToString("X")} from " +
                    $"0x{address.ToString($"X{IntPtr.Size}")}[Size: {size}]");
            return memInfo;
        }
    }
}
