using System;
using System.Collections.Generic;
using System.Linq;

namespace Essenbee.Z80
{
    public class Instruction
    {
        public string Mnemonic { get; set; }
        public Func<byte> AddressingMode1 { get; set; }
        public Func<byte> AddressingMode2 { get; set; }
        public Func<byte, byte> Op { get; set; }
        public int TStates { get; set; }
        public int MCycles { get; set; }
        public List<int> Timing { get; set; }

        public Instruction(string mnemonic, Func<byte> addrMode1, Func<byte> addrMode2, Func<byte, byte> op, 
            List<int> timing)
        {
            Mnemonic = mnemonic;
            AddressingMode1 = addrMode1;
            AddressingMode2 = addrMode2;
            Op = op;
            Timing = timing;
            TStates = timing.Sum();
            MCycles = timing.Count;
        }
    }
}
    