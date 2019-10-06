using System;
using System.Collections.Generic;

namespace Essenbee.Z80
{
    public partial class Z80
    {
        [Flags]
        public enum Flags
        {
            // Carry flag - set if an ADD instruction generates a carry or a SUB instruction generates a borrow.
            C = (1 << 0),
            // Add/Subtract flag - used by the DAA instruction; set for Subtractions, cleared for Additions.
            N = (1 << 1),
            // Parity/Overflow flag - for arithmetic operations, this flag is set when overflow occurs.
            // Also usd in logical operations to indicate that the resulting parity is even
            P = (1 << 2),
            // Not Used
            X = (1 << 3),
            // Half Carry flag - set or cleared depending upon the carry/borrow status between bits 3
            // and 4 of an 8-bit arithmetic operation
            H = (1 << 4),
            // Not Used
            U = (1 << 5),
            // Zero flag - set if the result of an arithmetic operation is zero.
            Z = (1 << 6),
            // Sign flag - stores the sate of the MSB of the Accumulator (register A).
            S = (1 << 7),
        };

        // CPU Registers
        //`
        //` ![](E5D9CCCCCD8B82BB1159F822CFE086F8.png;;;0.03343,0.03512)
        //`
        // ========================================
        // General Purpose Registers
        // ========================================
        public byte A { get; set; } = 0x00;
        public Flags F { get; set; } = 0x00; //Shows CPU state in the form of bit flags
        public byte B { get; set; } = 0x00;
        public byte C { get; set; } = 0x00;
        public byte D { get; set; } = 0x00;
        public byte E { get; set; } = 0x00;
        public byte H { get; set; } = 0x00;
        public byte L { get; set; } = 0x00;
        // ========================================
        // Shadow General Purpose Registers
        // ========================================
        public byte A1 { get; set; } = 0x00;
        public Flags F1 { get; set; } = 0x00;
        public byte B1 { get; set; } = 0x00;
        public byte C1 { get; set; } = 0x00;
        public byte D1 { get; set; } = 0x00;
        public byte E1 { get; set; } = 0x00;
        public byte H1 { get; set; } = 0x00;
        public byte L1 { get; set; } = 0x00;

        // ========================================
        // Special Purpose Registers
        // ========================================
        public byte I { get; set; } = 0x00;
        public byte R { get; set; } = 0x00;
        // Index Register X
        public ushort IX { get; set; } = 0x0000;
        // Index Register Y
        public ushort IY { get; set; } = 0x0000;
        // Stack Pointer
        public ushort SP { get; set; } = 0x0000;
        // Program Counter
        public ushort PC { get; set; } = 0x0000;

        // 16-bit Combined Registers
        public ushort HL => (ushort)(L + (H << 8));
        public ushort BC => (ushort)((B << 8) + C);
        public ushort DE => (ushort)((D << 8) + E);

        private IBus _bus;
        private Dictionary<byte, Instruction> _rootInstructions = new Dictionary<byte, Instruction>();
        private ushort _absoluteAddress = 0x0000;
        private ushort _relativeAddress = 0x0000;

        public Z80()
        {
        }

        public void ConnectToBus(IBus bus)
        {
            _bus = bus;
        }

        private byte ReadFromBus(ushort addr)
        {
            return _bus.Read(addr, false);
        }

        private void WriteToBus(ushort addr, byte data)
        {
            _bus.Write(addr, data);
        }

        private bool CheckFlag(Flags flag, bool isAlternate = false)
        {
            if (!isAlternate)
            {
                if ((F & flag) == flag)
                {
                    return true;
                }
            }
            else
            {
                if ((F1 & flag) == flag)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetFlag(Flags flag, bool value, bool isAlternate = false)
        {
            if (!isAlternate)
            {
                if (value)
                {
                    F |= flag;
                }
                else
                {
                    F &= ~flag;
                }
            }
            else
            {
                if (value)
                {
                    F1 |= flag;
                }
                else
                {
                    F1 &= ~flag;
                }
            }
        }
    }
}
