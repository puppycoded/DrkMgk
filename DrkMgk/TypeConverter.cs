using DrkMgk;
using System;
using System.Runtime.InteropServices;

namespace DrkMgk
{
    public static unsafe class TypeConverter
    {
        public static IntPtr BytesToPtr(in byte[] bytes)
        {
            fixed (byte* bytesPtr = bytes)
            {
                return (IntPtr)bytesPtr;
            }
        }

        public static IntPtr ValueToPtr<T>(in T value) where T : struct
        {
            return (IntPtr)MarshalType<T>.GetPointer(value);
        }

        public static byte[] PtrToBytes(IntPtr ptr, int size)
        {
            byte[] bytes = new byte[size];
            fixed (byte* bytesPtr = bytes)
            {
                Native.Copy(bytesPtr, ptr.ToPointer(), size);
                return bytes;
            }
        }

        public static T PtrToValue<T>(IntPtr ptr) where T : struct
        {
            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    if (MarshalType<T>.IsIntPtr)
                        return (T)(object)*(IntPtr*)ptr;
                    break;
                case TypeCode.Boolean:
                    return (T)(object)*(bool*)ptr;
                case TypeCode.SByte:
                    return (T)(object)*(sbyte*)ptr;
                case TypeCode.Byte:
                    return (T)(object)*(byte*)ptr;
                case TypeCode.Int16:
                    return (T)(object)*(short*)ptr;
                case TypeCode.UInt16:
                    return (T)(object)*(ushort*)ptr;
                case TypeCode.Int32:
                    return (T)(object)*(int*)ptr;
                case TypeCode.UInt32:
                    return (T)(object)*(int*)ptr;
                case TypeCode.Int64:
                    return (T)(object)*(long*)ptr;
                case TypeCode.UInt64:
                    return (T)(object)*(ulong*)ptr;
                case TypeCode.Single:
                    return (T)(object)*(float*)ptr;
                case TypeCode.Double:
                    return (T)(object)*(double*)ptr;
            }

            if (!MarshalType<T>.HasUnmanagedTypes)
            {
                T value = default(T);
                void* valuePtr = MarshalType<T>.GetPointer(value);
                Native.Copy(valuePtr, ptr.ToPointer(), MarshalType<T>.Size);
                return value;
            }

            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        public static byte[] ValueToBytes<T>(T value) where T : struct
        {
            int size = MarshalType<T>.Size;

            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    if (MarshalType<T>.IsIntPtr)
                    {
                        switch (size)
                        {
                            case 4:
                                return BitConverter.GetBytes(((IntPtr)(object)value).ToInt32());
                            case 8:
                                return BitConverter.GetBytes(((IntPtr)(object)value).ToInt64());
                        }
                    }
                    break;
                case TypeCode.Boolean:
                    return BitConverter.GetBytes((bool)(object)value);
                case TypeCode.SByte:
                    return BitConverter.GetBytes((sbyte)(object)value);
                case TypeCode.Byte:
                    return BitConverter.GetBytes((byte)(object)value);
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)(object)value);
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)(object)value);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)(object)value);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((int)(object)value);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)(object)value);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)(object)value);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)(object)value);
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)(object)value);
            }

            byte[] bytes = new byte[size];

            if (!MarshalType<T>.HasUnmanagedTypes)
            {
                void* valuePtr = MarshalType<T>.GetPointer(value);
                fixed (byte* bytesPtr = bytes)
                {
                    Native.Copy(bytesPtr, valuePtr, size);
                    return bytes;
                }
            }

            using (var memory = new LocalAlloc(size))
            {
                memory.Write(value);
                bytes = memory.Read();
            }

            return bytes;
        }

        public static T BytesToValue<T>(in byte[] bytes) where T : struct
        {
            int size = MarshalType<T>.Size;

            switch (MarshalType<T>.TypeCode)
            {
                case TypeCode.Object:
                    if (MarshalType<T>.IsIntPtr)
                    {
                        switch (bytes.Length)
                        {
                            case 1:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[] { bytes[0], 0, 0, 0 }, 0));
                            case 2:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], 0, 0 }, 0));
                            case 4:
                                return (T)(object)new IntPtr(BitConverter.ToInt32(bytes, 0));
                            case 8:
                                return (T)(object)new IntPtr(BitConverter.ToInt64(bytes, 0));
                        }
                    }
                    break;
                case TypeCode.Boolean:
                    return (T)(object)BitConverter.ToBoolean(bytes, 0);
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return (T)(object)bytes[0];
                case TypeCode.Int16:
                    return (T)(object)BitConverter.ToInt16(bytes, 0);
                case TypeCode.UInt16:
                    return (T)(object)BitConverter.ToUInt16(bytes, 0);
                case TypeCode.Int32:
                    return (T)(object)BitConverter.ToInt32(bytes, 0);
                case TypeCode.UInt32:
                    return (T)(object)BitConverter.ToUInt32(bytes, 0);
                case TypeCode.Int64:
                    return (T)(object)BitConverter.ToInt64(bytes, 0);
                case TypeCode.UInt64:
                    return (T)(object)BitConverter.ToUInt64(bytes, 0);
                case TypeCode.Single:
                    return (T)(object)BitConverter.ToSingle(bytes, 0);
                case TypeCode.Double:
                    return (T)(object)BitConverter.ToDouble(bytes, 0);
            }

            T value = default(T);

            if (!MarshalType<T>.HasUnmanagedTypes)
            {
                void* valuePtr = MarshalType<T>.GetPointer(value);
                fixed (byte* bytesPtr = bytes)
                {
                    Native.Copy(valuePtr, bytesPtr, size);
                    return value;
                }
            }

            using (var memory = new LocalAlloc(size))
            {
                memory.Write(bytes);
                value = memory.Read<T>();
            }

            return value;
        }
    }
}
