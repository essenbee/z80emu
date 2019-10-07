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
        private byte _currentOpCode = 0x00;
        private int _clockCycles = 0;

        public Z80()
        {
            _rootInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("NOP", IMP, IMP, NOP, 4) },

                { 0x06, new Instruction("LD B,n", IMM, IMM, LDRN, 7) },
                { 0x0E, new Instruction("LD C,n", IMM, IMM, LDRN, 7) },
                { 0x16, new Instruction("LD D,n", IMM, IMM, LDRN, 7) },
                { 0x1E, new Instruction("LD E,n", IMM, IMM, LDRN, 7) },
                { 0x26, new Instruction("LD H,n", IMM, IMM, LDRN, 7) },
                { 0x2E, new Instruction("LD L,n", IMM, IMM, LDRN, 7) },
                { 0x3E, new Instruction("LD A,n", IMM, IMM, LDRN, 7) },

                { 0x40, new Instruction("LD B,B", IMP, IMP, LDRR, 4) },
                { 0x41, new Instruction("LD B,C", IMP, IMP, LDRR, 4) },
                { 0x42, new Instruction("LD B,D", IMP, IMP, LDRR, 4) },
                { 0x43, new Instruction("LD B,E", IMP, IMP, LDRR, 4) },
                { 0x44, new Instruction("LD B,H", IMP, IMP, LDRR, 4) },
                { 0x45, new Instruction("LD B,L", IMP, IMP, LDRR, 4) },
                { 0x47, new Instruction("LD B,A", IMP, IMP, LDRR, 4) },

                { 0x48, new Instruction("LD C,B", IMP, IMP, LDRR, 4) },
                { 0x49, new Instruction("LD C,C", IMP, IMP, LDRR, 4) },
                { 0x4A, new Instruction("LD C,D", IMP, IMP, LDRR, 4) },
                { 0x4B, new Instruction("LD C,E", IMP, IMP, LDRR, 4) },
                { 0x4C, new Instruction("LD C,H", IMP, IMP, LDRR, 4) },
                { 0x4D, new Instruction("LD C,L", IMP, IMP, LDRR, 4) },
                { 0x4F, new Instruction("LD C,A", IMP, IMP, LDRR, 4) },

                { 0x50, new Instruction("LD D,B", IMP, IMP, LDRR, 4) },
                { 0x51, new Instruction("LD D,C", IMP, IMP, LDRR, 4) },
                { 0x52, new Instruction("LD D,D", IMP, IMP, LDRR, 4) },
                { 0x53, new Instruction("LD D,E", IMP, IMP, LDRR, 4) },
                { 0x54, new Instruction("LD D,H", IMP, IMP, LDRR, 4) },
                { 0x55, new Instruction("LD D,L", IMP, IMP, LDRR, 4) },
                { 0x57, new Instruction("LD D,A", IMP, IMP, LDRR, 4) },

                { 0x58, new Instruction("LD E,B", IMP, IMP, LDRR, 4) },
                { 0x59, new Instruction("LD E,C", IMP, IMP, LDRR, 4) },
                { 0x5A, new Instruction("LD E,D", IMP, IMP, LDRR, 4) },
                { 0x5B, new Instruction("LD E,E", IMP, IMP, LDRR, 4) },
                { 0x5C, new Instruction("LD E,H", IMP, IMP, LDRR, 4) },
                { 0x5D, new Instruction("LD E,L", IMP, IMP, LDRR, 4) },
                { 0x5F, new Instruction("LD E,A", IMP, IMP, LDRR, 4) },

                { 0x60, new Instruction("LD H,B", IMP, IMP, LDRR, 4) },
                { 0x61, new Instruction("LD H,C", IMP, IMP, LDRR, 4) },
                { 0x62, new Instruction("LD H,D", IMP, IMP, LDRR, 4) },
                { 0x63, new Instruction("LD H,E", IMP, IMP, LDRR, 4) },
                { 0x64, new Instruction("LD H,H", IMP, IMP, LDRR, 4) },
                { 0x65, new Instruction("LD H,L", IMP, IMP, LDRR, 4) },
                { 0x67, new Instruction("LD H,A", IMP, IMP, LDRR, 4) },

                { 0x68, new Instruction("LD L,B", IMP, IMP, LDRR, 4) },
                { 0x69, new Instruction("LD L,C", IMP, IMP, LDRR, 4) },
                { 0x6A, new Instruction("LD L,D", IMP, IMP, LDRR, 4) },
                { 0x6B, new Instruction("LD L,E", IMP, IMP, LDRR, 4) },
                { 0x6C, new Instruction("LD L,H", IMP, IMP, LDRR, 4) },
                { 0x6D, new Instruction("LD L,L", IMP, IMP, LDRR, 4) },
                { 0x6F, new Instruction("LD L,A", IMP, IMP, LDRR, 4) },

                { 0x76, new Instruction("HALT", IMP, IMP, HALT, 4) },

                { 0x78, new Instruction("LD A,B", IMP, IMP, LDRR, 4) },
                { 0x79, new Instruction("LD A,C", IMP, IMP, LDRR, 4) },
                { 0x7A, new Instruction("LD A,D", IMP, IMP, LDRR, 4) },
                { 0x7B, new Instruction("LD A,E", IMP, IMP, LDRR, 4) },
                { 0x7C, new Instruction("LD A,H", IMP, IMP, LDRR, 4) },
                { 0x7D, new Instruction("LD A,L", IMP, IMP, LDRR, 4) },
                { 0x7F, new Instruction("LD A,A", IMP, IMP, LDRR, 4) },
            };
        }

        public void ConnectToBus(IBus bus)
        {
            _bus = bus;
        }

        public void Tick()
        {
            if (_clockCycles == 0)
            {
                _currentOpCode = ReadFromBus(PC);
                PC++;
                _clockCycles = _rootInstructions[_currentOpCode].TCycles;
                _rootInstructions[_currentOpCode].Op(_currentOpCode);
                _clockCycles--;
            }
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

        private byte Fetch1()
        {
            if (!(_rootInstructions[_currentOpCode].AddressingMode1 == IMP))
            {
                return ReadFromBus(_absoluteAddress);
            }

            return 0x00;
        }
    }
}
