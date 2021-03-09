using System;
using System.Text;

namespace DrkMgk
{
    public static class MemoryLiterate
    {
        public static byte[] Read(IntPtr address, int size)
        {
            return TypeConverter.PtrToBytes(address, size);
        }

        public static T Read<T>(IntPtr address) where T : struct
        {
            return TypeConverter.BytesToValue<T>(Read(address, MarshalType<T>.Size));
        }

        public static byte[] Read(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            Native.ReadProcessMemory(processHandle, address, buffer, size);
            return buffer;
        }

        public static T Read<T>(SafeMemoryHandle processHandle, IntPtr address) where T : struct
        {
            return TypeConverter.BytesToValue<T>(Read(processHandle, address, MarshalType<T>.Size));
        }

        public static string Read(SafeMemoryHandle processHandle, IntPtr address, int size, in Encoding encoding)
        {
            byte[] buffer = Read(processHandle, address, size);
            string s = encoding.GetString(buffer);
            int i = s.IndexOf('\0');
            if (i != -1)
                s = s.Remove(i);
            return s;
        }

        public static void Write(IntPtr address, in byte[] bytes)
        {
            Native.Copy(address, TypeConverter.BytesToPtr(bytes), bytes.Length);
        }

        public static void Write<T>(IntPtr address, T value) where T : struct
        {
            Write(address, TypeConverter.ValueToBytes(value));
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, in byte[] bytes)
        {
            using (new MemoryProtection(processHandle, address, bytes.Length))
                return Native.WriteProcessMemory(processHandle, address, bytes, bytes.Length) == bytes.Length;
        }

        public static bool Write<T>(SafeMemoryHandle processHandle, IntPtr address, T value) where T : struct
        {
            return Write(processHandle, address, TypeConverter.ValueToBytes(value));
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, in string value, in Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(value);
            return Write(processHandle, address, bytes);
        }
    }
}
