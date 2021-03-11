using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DrkMgk
{
    public class Signature : IDisposable
    {
        public SafeMemoryHandle ProcessHandle { get; private set; }
        public string String { get; private set; }
        public byte[] Bytes { get; private set; }
        public byte[] BytesMask { get; private set; }
        public IntPtr Address { get; private set; }

        public Signature(SafeMemoryHandle processHandle, string signature)
        {
            ProcessHandle = processHandle;
            String = signature.ToUpper();
            string sSig = string.Join("", String.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            if (sSig.Length % 2 != 0)
                throw new ArgumentException("Signature contains an invalid number of characters.");

            Bytes = new byte[sSig.Length / 2];
            BytesMask = new byte[Bytes.Length];

            Func<char, bool> IsValidCharacter = delegate (char c)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c == '?'))
                    return true;
                return false;
            };

            for (int i = 0; i < sSig.Length; i += 2)
            {
                string sByte = $"{sSig[i]}{sSig[i + 1]}";
                if (!IsValidCharacter(sByte[0]) || !IsValidCharacter(sByte[1]))
                    throw new ArgumentException("Signature contains invalid characters.");

                if (sByte == "??")
                {
                    Bytes[i / 2] = 0x0;
                    BytesMask[i / 2] = 0x0;
                }
                else
                {
                    Bytes[i / 2] = byte.Parse(sByte, NumberStyles.HexNumber);
                    BytesMask[i / 2] = 0x1;
                }
            }
        }

        ~Signature()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void Scan(ref List<IntPtr> results, in byte[] buffer)
        {
            for (int i = 0, j = 0; i < buffer.Length; ++i)
            {
                if (buffer[i] == Bytes[j] || BytesMask[j] == 0x0)
                {
                    ++j;

                    if (j == Bytes.Length)
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
        }

        private void ScanRegion(ref List<IntPtr> results, in MemoryBasicInformation region)
        {
            List<IntPtr> addresses = new List<IntPtr>();
            Scan(ref addresses, MemoryLiterate.Read(ProcessHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
            {
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));
            }
        }

        private void ScanRegions(ref List<IntPtr> results, in List<MemoryBasicInformation> regions)
        {
            foreach (MemoryBasicInformation region in regions)
            {
                ScanRegion(ref results, region);
            }

        }

        private void ScanModule(ref List<IntPtr> results, in ProcessModule module)
        {
            ScanRegions(ref results, MemoryRegion.LoadRegions(ProcessHandle, module));
        }

        private void ScanAllModules(ref List<IntPtr> results, in ProcessModuleCollection modules)
        {
            foreach (ProcessModule module in modules)
            {
                ScanModule(ref results, module);
            }
        }

        private void ScanRange(ref List<IntPtr> results, IntPtr startAddress, IntPtr endAddress)
        {
            ScanRegions(ref results, MemoryRegion.LoadRegions(ProcessHandle, startAddress, endAddress));
        }

        private void Rescan(ref List<IntPtr> results, IntPtr address)
        {
            Scan(ref results, MemoryLiterate.Read(ProcessHandle, address, Bytes.Length));
        }

        public bool Scan(in byte[] buffer)
        {
            List<IntPtr> results = new List<IntPtr>();
            Scan(ref results, buffer);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool ScanRegion(in MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            ScanRegion(ref results, region);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool ScanRegions(in List<MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            ScanRegions(ref results, regions);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool ScanModule(in ProcessModule module)
        {
            List<IntPtr> results = new List<IntPtr>();
            ScanModule(ref results, module);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool ScanAllModules(in ProcessModuleCollection modules)
        {
            List<IntPtr> results = new List<IntPtr>();
            ScanAllModules(ref results, modules);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool ScanRange(IntPtr startAddress, IntPtr endAddress)
        {
            List<IntPtr> results = new List<IntPtr>();
            ScanRange(ref results, startAddress, endAddress);
            if (results.Count == 1)
            {
                Address = results[0];
                return true;
            }

            return false;
        }

        public bool Rescan(IntPtr address)
        {
            List<IntPtr> results = new List<IntPtr>();
            Rescan(ref results, address);
            if (results.Count == 1)
            {
                return true;
            }

            Address = IntPtr.Zero;
            return false;
        }

        public IntPtr ResolveAddressFromBytes()
        {
            switch (IntPtr.Size)
            {
                case 4: return MemoryLiterate.Read<IntPtr>(ProcessHandle, Address);
                case 8: return new IntPtr(Address.ToInt64() + MemoryLiterate.Read<uint>(ProcessHandle, Address) + 4);
                default: return IntPtr.Zero;
            }
        }
    }
}
