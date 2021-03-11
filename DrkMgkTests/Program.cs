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
            try
            {
                string processName = "DarkSoulsRemastered";
                Process[] results = Process.GetProcessesByName(processName);
                Process process = results[0];
                SafeMemoryHandle processHandle = Native.OpenProcess(process.Id);

                Func<IntPtr, IntPtr> resolveAddress64 = delegate (IntPtr address)
                {
                    return IntPtr.Add(address, MemoryLiterate.Read<IntPtr>(processHandle, address).ToInt32() + 4);
                };

                Signature dropItemLuaAob = new Signature(processHandle, "48 ?? ?? ?? ?? 48 ?? ?? ?? ?? 57 48 ?? ?? ?? ?? ?? ?? 0F B6 ?? 48 ?? ?? E8");
                Signature dropItemBaseAob = new Signature(processHandle, "?? ?? ?? ?? 33 ?? E8 ?? ?? ?? ?? 80 ?? ?? ?? 44 ?? ?? 48 ?? ?? ?? ?? ?? ?? 8B");
                Signature dtemDbgBaseAob = new Signature(processHandle, "?? ?? ?? ?? 89 ?? 3C 08 00 00 8B ?? ?? 89 ?? 40 08 00 00 8B ?? ?? 89 ?? 44 08 00 00 8B ?? ?? 89 ?? 48 08 00 00 C3");
                dropItemLuaAob.ScanModule(process.MainModule);
                dropItemBaseAob.ScanModule(process.MainModule);
                dtemDbgBaseAob.ScanModule(process.MainModule);
                IntPtr dropItemLua = dropItemLuaAob.Address;
                IntPtr dropItemBase = dropItemBaseAob.ResolveAddressFromBytes();
                IntPtr itemDbgBase = dtemDbgBaseAob.ResolveAddressFromBytes();
                Pointer itemCategoryPtr = new Pointer(processHandle, itemDbgBase, 0x83C);
                Pointer itemIdPtr = new Pointer(processHandle, itemDbgBase, 0x840);
                Pointer itemDurabilityPtr = new Pointer(processHandle, itemDbgBase, 0x844);
                Pointer itemQuantityPtr = new Pointer(processHandle, itemDbgBase, 0x848);
                itemCategoryPtr.TryWrite(0x40000000);
                itemIdPtr.TryWrite(0x15E);
                itemDurabilityPtr.TryWrite(-1);
                itemQuantityPtr.TryWrite(1);
                string[] asm = new string[]
                {
                    $"mov rax,0x{dropItemBase.ToString("X")}",
                    "mov rcx,[rax]",
                    $"mov rax, 0x{dropItemLua.ToString("X")}",
                    "sub rsp,0x38",
                    "call rax",
                    "add rsp,0x38",
                    "ret"
                };
                byte[] byteCode = Assembler.Assemble64(asm);
                using (var memory = new RemoteAlloc(processHandle, byteCode.Length))
                {
                    memory.Write(byteCode);
                    SafeMemoryHandle threadHandle = Native.CreateThread(processHandle, memory.AllocationBase, IntPtr.Zero);
                    Native.WaitForSingleObject(threadHandle);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadKey();
        }
    }
}
