using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DrkMgk
{
    public static class MemoryScanner
    {
        public static List<IntPtr> ScanForBytes(in byte[] bytes, in byte[] buffer)
        {
            List<IntPtr> results = new List<IntPtr>();

            for (int i = 0, j = 0; i < buffer.Length; ++i)
            {
                if (buffer[i] == bytes[j])
                {
                    ++j;

                    if (j == bytes.Length)
                    {
                        results.Add(new IntPtr(i - j + 1));
                        break;
                    }
                }
                else
                {
                    i -= j;
                    j = 0;
                }
            }

            return results;
        }

        public static List<IntPtr> ScanForValue<T>(T value, in byte[] buffer) where T : struct
        {
            return ScanForBytes(TypeConverter.ValueToBytes(value), buffer);
        }

        public static List<IntPtr> ScanRegionForBytes(SafeMemoryHandle processHandle, in byte[] bytes, in MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForBytes(bytes, MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionForValue<T>(SafeMemoryHandle processHandle, T value, in MemoryBasicInformation region) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForValue(value, MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionsForBytes(SafeMemoryHandle processHandle, in byte[] bytes, in List<MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForBytes(processHandle, bytes, region));

            return results;
        }

        public static List<IntPtr> ScanRegionsForValue<T>(SafeMemoryHandle processHandle, T value, in List<MemoryBasicInformation> regions) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForValue(processHandle, value, region));

            return results;
        }

        public static List<IntPtr> ScanModuleForBytes(in Process process, SafeMemoryHandle processHandle, in byte[] bytes, in ProcessModule module)
        {
            return ScanRegionsForBytes(processHandle, bytes, MemoryRegion.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanModuleForValue<T>(in Process process, SafeMemoryHandle processHandle, T value, in ProcessModule module) where T : struct
        {
            return ScanRegionsForValue(processHandle, value, MemoryRegion.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanAllModulesForBytes(in Process process, SafeMemoryHandle processHandle, in byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForBytes(process, processHandle, bytes, module))
                    results.Add(address);

            return results;
        }

        public static List<IntPtr> ScanAllModulesForValue<T>(in Process process, SafeMemoryHandle processHandle, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForValue(process, processHandle, value, module))
                    results.Add(address);

            return results;
        }

        public static IntPtr RescanForBytes(SafeMemoryHandle processHandle, in byte[] bytes, IntPtr address)
        {
            if (MemoryLiterate.Read(processHandle, address, bytes.Length) == bytes)
                return address;

            return IntPtr.Zero;
        }

        public static IntPtr RescanForValue<T>(SafeMemoryHandle processHandle, T value, IntPtr address) where T : struct
        {
            return RescanForBytes(processHandle, TypeConverter.ValueToBytes(value), address);
        }

        public static List<IntPtr> RescanForBytes(SafeMemoryHandle processHandle, in byte[] bytes, in List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForBytes(processHandle, bytes, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForValue<T>(SafeMemoryHandle processHandle, T value, in List<IntPtr> addresses) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForValue(processHandle, value, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }
    }
}
