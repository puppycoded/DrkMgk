using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DrkMgk
{
    public static class MemoryRegion
    {
        public static List<MemoryBasicInformation> LoadRegions(SafeMemoryHandle processHandle, IntPtr startAddress, IntPtr endAddress,
            MemoryAllocationState memoryAllocationState = MemoryAllocationState.MEM_COMMIT,
            MemoryProtectionType memoryProtectionType = MemoryProtectionType.PAGE_ACCESSIBLE)
        {
            List<MemoryBasicInformation> regions = new List<MemoryBasicInformation>();

            if (startAddress == endAddress)
                return regions;

            if (endAddress == IntPtr.Zero)
                endAddress = new IntPtr(0x7fffffffffff);

            IntPtr seek = startAddress;

            do
            {
                MemoryBasicInformation region = Native.Query(processHandle, seek, MarshalType<MemoryBasicInformation>.Size);
                if ((region.State & memoryAllocationState) != 0 && (region.Protect & memoryProtectionType) != 0)
                    regions.Add(region);

                seek = IntPtr.Add(region.BaseAddress, region.RegionSize.ToInt32());
            }
            while (seek.ToInt64() < endAddress.ToInt64());

            return regions;
        }

        public static List<MemoryBasicInformation> LoadRegions(SafeMemoryHandle processHandle, in ProcessModule processModule,
            MemoryAllocationState memoryAllocationState = MemoryAllocationState.MEM_COMMIT,
            MemoryProtectionType memoryProtectionType = MemoryProtectionType.PAGE_ACCESSIBLE)
        {
            return LoadRegions(processHandle, processModule.BaseAddress,
                IntPtr.Add(processModule.BaseAddress, processModule.ModuleMemorySize), memoryAllocationState, memoryProtectionType);
        }

        public static List<MemoryBasicInformation> LoadRegions(SafeMemoryHandle processHandle, in ProcessModuleCollection processModules,
            MemoryAllocationState memoryAllocationState = MemoryAllocationState.MEM_COMMIT,
            MemoryProtectionType memoryProtectionType = MemoryProtectionType.PAGE_ACCESSIBLE)
        {
            List<MemoryBasicInformation> regions = new List<MemoryBasicInformation>();

            foreach (ProcessModule processModule in processModules)
            {
                regions.AddRange(LoadRegions(processHandle, processModule, memoryAllocationState, memoryProtectionType));
            }

            return regions;
        }

        public static List<MemoryBasicInformation> LoadRegions(SafeMemoryHandle processHandle, in Process process,
            MemoryAllocationState memoryAllocationState = MemoryAllocationState.MEM_COMMIT,
            MemoryProtectionType memoryProtectionType = MemoryProtectionType.PAGE_ACCESSIBLE)
        {
            return LoadRegions(processHandle, process.Modules, memoryAllocationState, memoryProtectionType);
        }
    }
}
