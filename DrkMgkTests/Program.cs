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

            Pointer healthPtr = new Pointer(processHandle, process.MainModule.BaseAddress, 0x1D151B0, 0x68, 0x3E8);

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
