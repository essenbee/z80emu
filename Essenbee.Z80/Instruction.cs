using System;
using System.Collections.Generic;
using System.Text;

namespace Essenbee.Z80
{
    public class Instruction
    {
        public string Mnemonic { get; set; }
        public Func<byte> AddressingMode1 { get; set; }
        public Func<byte> AddressingMode2 { get; set; }
        public Func<byte> Op { get; set; }
        public int TCycles { get; set; }

        public Instruction(string mnemonic, Func<byte> addrMode1, Func<byte> addrMode2, Func<byte> op, int cycles)
        {
            Mnemonic = mnemonic;
            AddressingMode1 = addrMode1;
            AddressingMode2 = addrMode2;
            Op = op;
            TCycles = cycles;
        }
    }
}
    