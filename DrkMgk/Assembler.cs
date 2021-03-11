using System;
using System.Collections.Generic;

namespace DrkMgk
{
    public static class Assembler
    {
        private static readonly Reloaded.Assembler.Assembler Asm = new Reloaded.Assembler.Assembler();

        public static byte[] Assemble32(in IEnumerable<string> mnemonics)
        {
            string mnemonicsString = string.Join(Environment.NewLine, mnemonics);
            return Asm.Assemble($"use32 {mnemonicsString}");
        }

        public static byte[] Assemble32(in string mnemonics)
        {
            return Asm.Assemble($"use32 {mnemonics}");
        }

        public static byte[] Assemble64(in IEnumerable<string> mnemonics)
        {
            string mnemonicsString = string.Join(Environment.NewLine, mnemonics);
            return Asm.Assemble($"use64 {mnemonicsString}");
        }

        public static byte[] Assemble64(in string mnemonics)
        {
            return Asm.Assemble($"use64 {mnemonics}");
        }
    }
}