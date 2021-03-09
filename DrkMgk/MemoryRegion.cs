using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DrkMgk
{
    public static class MemoryRegion
    {
        private static List<MemoryBasicInformation> Scan(SafeMemoryHandle processHandle, in ProcessModule[] processModules, int moduleIndex, int endModule)
        {
            List<MemoryBasicInformation> regions = new List<MemoryBasicInformation>();

            for (; moduleIndex < endModule; ++moduleIndex)
            {
                long start = processModules[moduleIndex].BaseAddress.ToInt64();
                long end = moduleIndex + 1 > processModules.Length - 1
                    ? processModules[moduleIndex].ModuleMemorySize + 1
                    : processModules[moduleIndex + 1].BaseAddress.ToInt64();
                long seek = start;

                do
                {
                    MemoryBasicInformation region = Native.Query(processHandle, new IntPtr(seek), MarshalType<MemoryBasicInformation>.Size);
                    if ((region.State & MemoryAllocationState.MEM_COMMIT) != 0 && (region.Protect & (MemoryProtectionType)0x701) == 0)
                        regions.Add(region);

                    seek = region.BaseAddress.ToInt64() + region.RegionSize.ToInt64();
                }
                while (seek < end);
            }

            return regions;
        }

        private static List<MemoryBasicInformation> ScanAll(SafeMemoryHandle processHandle, in ProcessModule[] processModules)
        {
            return Scan(processHandle, processModules, 0, processModules.Length);
        }

        public static List<MemoryBasicInformation> Load(in Process process, SafeMemoryHandle processHandle, in ProcessModule processModule)
        {
            string moduleName = processModule.ModuleName;
            ProcessModule[] processModules = GetProcessModules(process);
            int i = Array.FindIndex(processModules, m => m.ModuleName == moduleName);
            return Scan(processHandle, processModules, i, i + 1);
        }

        public static List<MemoryBasicInformation> LoadAll(in Process process, SafeMemoryHandle processHandle)
        {
            ProcessModule[] processModules = GetProcessModules(process);
            return ScanAll(processHandle, processModules);
        }

        private static ProcessModule[] GetProcessModules(in Process process)
        {
            ProcessModule[] processModules = new ProcessModule[process.Modules.Count];
            process.Modules.CopyTo(processModules, 0);
            return processModules.OrderBy(p => p.BaseAddress.ToInt64()).ToArray();
        }
    }
}
