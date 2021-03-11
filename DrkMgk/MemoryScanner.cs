using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DrkMgk
{
    public static class MemoryScanner
    {
        public static List<IntPtr> ScanForBytes(in byte[] buffer, in byte[] bytes)
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

        public static List<IntPtr> ScanForValue<T>(in byte[] buffer, T value) where T : struct
        {
            return ScanForBytes(buffer, TypeConverter.ValueToBytes(value));
        }

        public static List<IntPtr> ScanRegionForBytes(SafeMemoryHandle processHandle, in MemoryBasicInformation region, in byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForBytes(MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()), bytes);
            foreach (IntPtr address in addresses)
            {
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));
            }

            return results;
        }

        public static List<IntPtr> ScanRegionForValue<T>(SafeMemoryHandle processHandle, in MemoryBasicInformation region, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForValue(MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()), value);
            foreach (IntPtr address in addresses)
            {
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));
            }

            return results;
        }

        public static List<IntPtr> ScanRegionsForBytes(SafeMemoryHandle processHandle, in List<MemoryBasicInformation> regions, in byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
            {
                results.AddRange(ScanRegionForBytes(processHandle, region, bytes));
            }

            return results;
        }

        public static List<IntPtr> ScanRegionsForValue<T>(SafeMemoryHandle processHandle, in List<MemoryBasicInformation> regions, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
            {
                results.AddRange(ScanRegionForValue(processHandle, region, value));
            }

            return results;
        }

        public static List<IntPtr> ScanModuleForBytes(SafeMemoryHandle processHandle, in ProcessModule module, in byte[] bytes)
        {
            return ScanRegionsForBytes(processHandle, MemoryRegion.LoadRegions(processHandle, module), bytes);
        }

        public static List<IntPtr> ScanModuleForValue<T>(SafeMemoryHandle processHandle, in ProcessModule module, T value) where T : struct
        {
            return ScanRegionsForValue(processHandle, MemoryRegion.LoadRegions(processHandle, module), value);
        }

        public static List<IntPtr> ScanAllModulesForBytes(SafeMemoryHandle processHandle, in ProcessModuleCollection modules, in byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in modules)
            {
                foreach (IntPtr address in ScanModuleForBytes(processHandle, module, bytes))
                {
                    results.Add(address);
                }
            }

            return results;
        }

        public static List<IntPtr> ScanAllModulesForValue<T>(SafeMemoryHandle processHandle, in ProcessModuleCollection modules, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in modules)
            {
                foreach (IntPtr address in ScanModuleForValue(processHandle, module, value))
                {
                    results.Add(address);
                }
            }

            return results;
        }

        public static List<IntPtr> ScanRangeForBytes(SafeMemoryHandle processHandle, IntPtr startAddress, IntPtr endAddress, in byte[] bytes)
        {
            return ScanRegionsForBytes(processHandle, MemoryRegion.LoadRegions(processHandle, startAddress, endAddress), bytes);
        }

        public static List<IntPtr> ScanRangeForValue<T>(SafeMemoryHandle processHandle, IntPtr startAddress, IntPtr endAddress, T value) where T : struct
        {
            return ScanRegionsForValue(processHandle, MemoryRegion.LoadRegions(processHandle, startAddress, endAddress), value);
        }

        public static IntPtr RescanForBytes(SafeMemoryHandle processHandle, IntPtr address, in byte[] bytes)
        {
            if (MemoryLiterate.Read(processHandle, address, bytes.Length) == bytes)
                return address;

            return IntPtr.Zero;
        }

        public static IntPtr RescanForValue<T>(SafeMemoryHandle processHandle, IntPtr address, T value) where T : struct
        {
            return RescanForBytes(processHandle, address, TypeConverter.ValueToBytes(value));
        }

        public static List<IntPtr> RescanForBytes(SafeMemoryHandle processHandle, in List<IntPtr> addresses, in byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForBytes(processHandle, address, bytes);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForValue<T>(SafeMemoryHandle processHandle, in List<IntPtr> addresses, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForValue(processHandle, address, value);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }
    }
}
