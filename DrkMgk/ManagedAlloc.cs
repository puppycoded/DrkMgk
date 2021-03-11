using System;
using System.Runtime.InteropServices;

namespace DrkMgk
{
    public interface ManagedAlloc : IDisposable
    {
        byte[] Read();
        T Read<T>() where T : struct;
        void Write(byte[] bytes);
        void Write<T>(T value) where T : struct;
    }

    public class LocalAlloc : ManagedAlloc
    {
        public int Size { get; private set; }
        public IntPtr AllocationBase { get; private set; }

        public LocalAlloc(in int size)
        {
            Size = size;
            AllocationBase = Marshal.AllocHGlobal(Size);
        }

        ~LocalAlloc()
        {
            Dispose();
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(AllocationBase);
            AllocationBase = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public byte[] Read()
        {
            byte[] bytes = new byte[Size];
            Marshal.Copy(AllocationBase, bytes, 0, Size);
            return bytes;
        }

        public T Read<T>() where T : struct
        {
            return (T)Marshal.PtrToStructure(AllocationBase, typeof(T));
        }

        public void Write(byte[] bytes)
        {
            Marshal.Copy(bytes, 0, AllocationBase, bytes.Length);
        }

        public void Write<T>(T value) where T : struct
        {
            Marshal.StructureToPtr(value, AllocationBase, false);
        }
    }

    public class RemoteAlloc : ManagedAlloc
    {
        public int Size { get; private set; }
        public SafeMemoryHandle ProcessHandle { get; private set; }
        public IntPtr AllocationBase { get; private set; }

        public RemoteAlloc(int size, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = null;
            AllocationBase = Native.Alloc(address, Size);
        }

        public RemoteAlloc(SafeMemoryHandle processHandle, int size, [Optional] IntPtr address)
        {
            Size = size;
            ProcessHandle = processHandle;
            AllocationBase = Native.Alloc(ProcessHandle, address, Size);
        }

        ~RemoteAlloc()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (ProcessHandle == null)
            {
                Native.Free(AllocationBase);
            }
            else
            {
                Native.Free(ProcessHandle, AllocationBase);
            }

            ProcessHandle = null;
            AllocationBase = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public byte[] Read()
        {
            if (ProcessHandle == null)
            {
                return MemoryLiterate.Read(AllocationBase, Size);
            }
            else
            {
                return MemoryLiterate.Read(ProcessHandle, AllocationBase, Size);
            }
        }

        public T Read<T>() where T : struct
        {
            return TypeConverter.BytesToValue<T>(Read());
        }

        public void Write(byte[] bytes)
        {
            if (ProcessHandle == null)
            {
                MemoryLiterate.Write(AllocationBase, bytes);
            }
            else
            {
                MemoryLiterate.Write(ProcessHandle, AllocationBase, bytes);
            }
        }

        public void Write<T>(T value) where T : struct
        {
            Write(TypeConverter.ValueToBytes(value));
        }
    }
}
