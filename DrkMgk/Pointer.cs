using System;
using System.Collections.Generic;

namespace DrkMgk
{
    public class Pointer : IDisposable
    {
        public SafeMemoryHandle ProcessHandle { get; private set; }
        public IntPtr BaseAddress { get; private set; }
        public List<int> Offsets { get; private set; } = new List<int>();
        public IntPtr LastResolvedAddress { get; private set; }

        public Pointer(SafeMemoryHandle processHandle, IntPtr baseAddress, params int[] offsets)
        {
            ProcessHandle = processHandle;
            BaseAddress = baseAddress;
            
            foreach (int i in offsets)
            {
                Offsets.Add(i);
            }
        }

        ~Pointer()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IntPtr Resolve()
        {
            IntPtr address = MemoryLiterate.Read<IntPtr>(ProcessHandle, BaseAddress);

            for (int i = 0; i < Offsets.Count - 1; ++i)
            {
                address = MemoryLiterate.Read<IntPtr>(ProcessHandle, address + Offsets[i]);
            }

            address += Offsets[Offsets.Count - 1];
            LastResolvedAddress = address;
            return LastResolvedAddress;
        }

        public byte[] Read(int size)
        {
            return MemoryLiterate.Read(ProcessHandle, Resolve(), size);
        }

        public T Read<T>() where T : struct
        {
            return TypeConverter.BytesToValue<T>(Read(MarshalType<T>.Size));
        }

        public void Write(byte[] bytes)
        {
            MemoryLiterate.Write(ProcessHandle, Resolve(), bytes);
        }

        public void Write<T>(T value) where T : struct
        {
            Write(TypeConverter.ValueToBytes(value));
        }

        public byte[] TryRead(int size)
        {
            try
            {
                return MemoryLiterate.Read(ProcessHandle, LastResolvedAddress, size);
            }
            catch
            {
                return MemoryLiterate.Read(ProcessHandle, Resolve(), size);
            }
        }

        public T TryRead<T>() where T : struct
        {
            return TypeConverter.BytesToValue<T>(TryRead(MarshalType<T>.Size));
        }

        public void TryWrite(byte[] bytes)
        {
            try
            {
                MemoryLiterate.Write(ProcessHandle, LastResolvedAddress, bytes);
            }
            catch
            {
                MemoryLiterate.Write(ProcessHandle, Resolve(), bytes);
            }
        }

        public void TryWrite<T>(T value) where T : struct
        {
            TryWrite(TypeConverter.ValueToBytes(value));
        }
    }
}
