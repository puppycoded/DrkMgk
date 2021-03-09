using System;
using System.Collections.Generic;
using System.Diagnostics;
using DrkMgk;

namespace DrkMgkTests
{
    class Program
    {
        static void Main(string[] args)
        {
            string processName = "DarkSoulsRemastered";
            Process[] results = Process.GetProcessesByName(processName);
            if (results.Length == 0)
            {
                Console.WriteLine("No process with the name {0} found.", processName);
                Console.ReadKey();
                return;
            }

            Process process = results[0];
            SafeMemoryHandle processHandle = Native.OpenProcess(process.Id);
            if (processHandle.IsInvalid || processHandle.IsClosed)
            {
                Console.WriteLine("Unable to open process[{0}][{1}]", process.ProcessName, process.Id);
                Console.ReadKey();
                return;
            }

            Signature sigWorldChrBase = new Signature("?? ?? ?? ?? 0F 28 ?? E8 ?? ?? ?? ?? 48 ?? ?? 74 ?? 48 ?? ?? 48 ?? ?? 48");
            var scanResults = sigWorldChrBase.ScanModule(process, processHandle, process.MainModule);
            if (scanResults.Count == 0)
            {
                Console.WriteLine("Unable to addresses matching {0}", sigWorldChrBase);
                Console.ReadKey();
                return;
            }

            if (scanResults.Count > 1)
            {
                Console.WriteLine("Multiple addresses found for {0}", sigWorldChrBase);
                Console.ReadKey();
                return;
            }

            IntPtr worldChrBase = (IntPtr)(scanResults[0].ToInt64() + MemoryLiterate.Read<uint>(processHandle, scanResults[0]) + 4);
            Pointer healthPtr = new Pointer(processHandle, worldChrBase, 0x68, 0x3E8);

            try
            {
                int health = healthPtr.TryRead<int>();
                Console.WriteLine("Player Health: {0}", health);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }

            Console.ReadKey();
        }
    }
}
