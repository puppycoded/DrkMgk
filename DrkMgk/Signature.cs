using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DrkMgk
{
    public class Signature : IDisposable
    {
        public string String { get; private set; }
        public byte[] Bytes { get; private set; }
        public byte[] BytesMask { get; private set; }

        public Signature(string signature)
        {
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

        public List<IntPtr> Scan(in byte[] buffer)
        {
            List<IntPtr> results = new List<IntPtr>();

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

            return results;
        }

        public List<IntPtr> ScanRegion(SafeMemoryHandle processHandle, in MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = Scan(MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public List<IntPtr> ScanRegions(SafeMemoryHandle processHandle, in List<MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegion(processHandle, region));

            return results;
        }

        public List<IntPtr> ScanModule(in Process process, SafeMemoryHandle processHandle, in ProcessModule module)
        {
            return ScanRegions(processHandle, MemoryRegion.Load(process, processHandle, module));
        }

        public List<IntPtr> ScanAllModules(in Process process, SafeMemoryHandle processHandle)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModule(process, processHandle, module))
                    results.Add(address);

            return results;
        }

        public IntPtr Rescan(SafeMemoryHandle processHandle, IntPtr address)
        {
            if (Scan(MemoryLiterate.Read(processHandle, address, Bytes.Length / 2)).Count == 0)
                return IntPtr.Zero;

            return address;
        }

        public List<IntPtr> Rescan(SafeMemoryHandle processHandle, in List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = Rescan(processHandle, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }
    }
}
