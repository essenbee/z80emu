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
        public Func<byte> Operation { get; set; }
        public int TCycles { get; set; }
    }
}
    