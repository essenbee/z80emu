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
            C = 1 << 0,
            // Add/Subtract flag - used by the DAA instruction; set for Subtractions, cleared for Additions.
            N = 1 << 1,
            // Parity/Overflow flag - for arithmetic operations, this flag is set when overflow occurs.
            // Also usd in logical operations to indicate that the resulting parity is even
            P = 1 << 2,
            // Undocumented - holds a copy of bit 3 of the result
            X = 1 << 3,
            // Half Carry flag - set or cleared depending upon the carry/borrow status between bits 3
            // and 4 of an 8-bit arithmetic operation. Need for Binary Coded Decimal correction.
            H = 1 << 4,
            // Undocumented - holds a copy of bit 5 of the result
            U = 1 << 5,
            // Zero flag - set if the result of an arithmetic operation is zero.
            Z = 1 << 6,
            // Sign flag - stores the state of the MSB of the Accumulator (register A).
            S = 1 << 7,
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

        // 16-bit Combined Shadow Registers
        public ushort HL1 => (ushort)(L1 + (H1 << 8));
        public ushort BC1 => (ushort)((B1 << 8) + C1);
        public ushort DE1 => (ushort)((D1 << 8) + E1);

        // Interrupt Flip-flops
        public bool IFF1 { get; set; }
        public bool IFF2 { get; set; }
        public InterruptMode InterruptMode { get; set; } = InterruptMode.Mode0;

        private IBus _bus = null!;

        private Dictionary<byte, Instruction> _rootInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _cbInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _ddInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _ddcbInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _edInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _fdInstructions = new Dictionary<byte, Instruction>();
        private Dictionary<byte, Instruction> _fdcbInstructions = new Dictionary<byte, Instruction>();

        private ushort _absoluteAddress = 0x0000;
        private byte _currentOpCode = 0x00;
        private int _clockCycles = 0;

        public Z80()
        {
            _rootInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0x01, new Instruction("LD BC,nn", IMM, IMP, LDBCNN, new List<int>{ 4, 3, 3 }) },
                { 0x02, new Instruction("LD (BC),A", IMP, IMP, LDBCA, new List<int>{ 4, 3 }) },

                { 0x04, new Instruction("INC B", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x05, new Instruction("DEC B", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x06, new Instruction("LD B,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x0A, new Instruction("LD A,(BC)", IDX, IMP, LDABC, new List<int>{ 4, 3 }) },

                { 0x0C, new Instruction("INC C", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x0D, new Instruction("DEC C", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x0E, new Instruction("LD C,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x11, new Instruction("LD DE,nn", IMM, IMP, LDDENN, new List<int>{ 4, 3, 3 }) },
                { 0x12, new Instruction("LD (DE),A", IMP, IMP, LDDEA, new List<int>{ 4, 3 }) },

                { 0x14, new Instruction("INC D", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x15, new Instruction("DEC D", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x16, new Instruction("LD D,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x1A, new Instruction("LD A,(DE)", IDX, IMP, LDADE, new List<int>{ 4, 3 }) },

                { 0x1C, new Instruction("INC E", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x1D, new Instruction("DEC E", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x1E, new Instruction("LD E,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x21, new Instruction("LD HL,nn", IMM, IMP, LDHLNN, new List<int>{ 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),HL", IMM, IDX, LDNNHL, new List<int>{ 4, 3, 3, 3, 3 }) },

                { 0x24, new Instruction("INC H", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x25, new Instruction("DEC H", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x26, new Instruction("LD H,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x27, new Instruction("DAA", IMM, IMM, DAA, new List<int>{ 4 }) },
                { 0x2A, new Instruction("LD HL,(nn)", IMM, IDX, LDHLFNN, new List<int>{ 4, 3, 3, 3, 3 }) },

                { 0x2C, new Instruction("INC L", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x2D, new Instruction("DEC L", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x2E, new Instruction("LD L,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },

                { 0x2F, new Instruction("CPL", IMP, IMP, CPL, new List<int>{ 4 }) },

                { 0x31, new Instruction("LD SP,nn", IMM, IMP, LDSPNN, new List<int>{ 4, 3, 3 }) },
                { 0x32, new Instruction("LD (nn),A", IMM, IMP, LDNNA, new List<int>{ 4, 3, 3, 3 }) },

                { 0x34, new Instruction("INC (HL)", RGIHL, IMP, INCHL, new List<int>{ 4, 4, 3 }) },
                { 0x35, new Instruction("DEC (HL)", RGIHL, IMP, DECHL, new List<int>{ 4, 4, 3 }) },

                { 0x36, new Instruction("LD (HL),n", IMM, IMM, LDHLN, new List<int>{ 4, 3, 3 }) },
                { 0x37, new Instruction("SCF", IMP, IMP, SCF, new List<int>{ 4 }) },
                { 0x3A, new Instruction("LD A,(nn)", IMM, IDX, LDANN, new List<int>{ 4, 3, 3, 3 }) },

                { 0x3C, new Instruction("INC A", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x3D, new Instruction("DEC A", IMP, IMP, DECR, new List<int>{ 4 }) },

                { 0x3E, new Instruction("LD A,n", IMM, IMM, LDRN, new List<int>{ 4, 3 }) },
                { 0x3F, new Instruction("CCF", IMP, IMP, CCF, new List<int>{ 4 }) },

                { 0x40, new Instruction("LD B,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x41, new Instruction("LD B,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x42, new Instruction("LD B,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x43, new Instruction("LD B,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x44, new Instruction("LD B,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x45, new Instruction("LD B,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x46, new Instruction("LD B,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x47, new Instruction("LD B,A", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x48, new Instruction("LD C,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x49, new Instruction("LD C,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x4A, new Instruction("LD C,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x4B, new Instruction("LD C,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x4C, new Instruction("LD C,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x4D, new Instruction("LD C,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x4E, new Instruction("LD C,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x4F, new Instruction("LD C,A", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x50, new Instruction("LD D,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x51, new Instruction("LD D,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x52, new Instruction("LD D,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x53, new Instruction("LD D,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x54, new Instruction("LD D,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x55, new Instruction("LD D,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x56, new Instruction("LD D,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x57, new Instruction("LD D,A", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x58, new Instruction("LD E,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x59, new Instruction("LD E,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x5A, new Instruction("LD E,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x5B, new Instruction("LD E,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x5C, new Instruction("LD E,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x5D, new Instruction("LD E,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x5E, new Instruction("LD E,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x5F, new Instruction("LD E,A", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x60, new Instruction("LD H,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x61, new Instruction("LD H,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x62, new Instruction("LD H,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x63, new Instruction("LD H,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x64, new Instruction("LD H,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x65, new Instruction("LD H,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x66, new Instruction("LD H,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x67, new Instruction("LD H,A", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x68, new Instruction("LD L,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x69, new Instruction("LD L,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x6A, new Instruction("LD L,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x6B, new Instruction("LD L,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x6C, new Instruction("LD L,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x6D, new Instruction("LD L,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x6E, new Instruction("LD L,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x6F, new Instruction("LD L,A", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x70, new Instruction("LD (HL),B", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },
                { 0x71, new Instruction("LD (HL),C", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },
                { 0x72, new Instruction("LD (HL),D", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },
                { 0x73, new Instruction("LD (HL),E", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },
                { 0x74, new Instruction("LD (HL),H", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },
                { 0x75, new Instruction("LD (HL),L", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },

                { 0x76, new Instruction("HALT", IMP, IMP, HALT, new List<int>{ 4 }) },

                { 0x77, new Instruction("LD (HL),A", IMP, IMP, LDHLR, new List<int>{ 4, 3 }) },

                { 0x78, new Instruction("LD A,B", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x79, new Instruction("LD A,C", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x7A, new Instruction("LD A,D", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x7B, new Instruction("LD A,E", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x7C, new Instruction("LD A,H", REG, REG, LDRR, new List<int>{ 4 }) },
                { 0x7D, new Instruction("LD A,L", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x7E, new Instruction("LD A,(HL)", RGIHL, RGIHL, LDRHL, new List<int>{ 4, 3 }) },

                { 0x7F, new Instruction("LD A,A", REG, REG, LDRR, new List<int>{ 4 }) },

                { 0x80, new Instruction("ADD A,B", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x81, new Instruction("ADD A,C", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x82, new Instruction("ADD A,D", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x83, new Instruction("ADD A,E", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x84, new Instruction("ADD A,H", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x85, new Instruction("ADD A,L", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x86, new Instruction("ADD A,(HL)", RGIHL, RGIHL, ADDAHL, new List<int>{ 4, 3 }) },
                { 0x87, new Instruction("ADD A,A", REG, REG, ADDAR, new List<int>{ 4 }) },
                { 0x88, new Instruction("ADC A,B", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x89, new Instruction("ADC A,C", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x8A, new Instruction("ADC A,D", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x8B, new Instruction("ADC A,E", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x8C, new Instruction("ADC A,H", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x8D, new Instruction("ADC A,L", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x8E, new Instruction("ADC A,(HL)", RGIHL, RGIHL, ADCAHL, new List<int>{ 4, 3 }) },
                { 0x8F, new Instruction("ADC A,A", REG, REG, ADCAR, new List<int>{ 4 }) },
                { 0x90, new Instruction("SUB A,B", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x91, new Instruction("SUB A,C", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x92, new Instruction("SUB A,D", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x93, new Instruction("SUB A,E", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x94, new Instruction("SUB A,H", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x95, new Instruction("SUB A,L", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x96, new Instruction("SUB A,(HL)", RGIHL, RGIHL, SUBAHL, new List<int>{ 4, 3 }) },
                { 0x97, new Instruction("SUB A,A", REG, REG, SUBAR, new List<int>{ 4 }) },
                { 0x98, new Instruction("SBC A,B", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x99, new Instruction("SBC A,C", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x9A, new Instruction("SBC A,D", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x9B, new Instruction("SBC A,E", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x9C, new Instruction("SBC A,H", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x9D, new Instruction("SBC A,L", REG, REG, SBCAR, new List<int>{ 4 }) },
                { 0x9E, new Instruction("SBC A,(HL)", RGIHL, RGIHL, SBCAHL, new List<int>{ 4, 3 }) },
                { 0x9F, new Instruction("SBC A,A", REG, REG, SBCAR, new List<int>{ 4 }) },

                { 0xB8, new Instruction("CP B", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xB9, new Instruction("CP C", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBA, new Instruction("CP D", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBB, new Instruction("CP E", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBC, new Instruction("CP H", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBD, new Instruction("CP L", IMP, IMP, CPR, new List<int>{ 4 }) },

                { 0xBE, new Instruction("BP (HL)", RGIHL, IMP, CPHL, new List<int>{ 4, 3 }) },

                { 0xBF, new Instruction("CP A", IMP, IMP, CPR, new List<int>{ 4 }) },

                { 0xC1, new Instruction("POP BC", IMM, IMM, POPBC, new List<int>{ 4, 3, 3 }) },
                { 0xC5, new Instruction("PUSH BC", IMM, IMM, PUSHBC, new List<int>{ 5, 3, 3 }) },

                { 0xC6, new Instruction("ADD A,n", IMM, IMM, ADDAN, new List<int>{ 4, 3 }) },
                { 0xCE, new Instruction("ADC A,n", IMM, IMM, ADCAN, new List<int>{ 4, 3 }) },

                { 0xD1, new Instruction("POP DE", IMM, IMM, POPDE, new List<int>{ 4, 3, 3 }) },
                { 0xD5, new Instruction("PUSH DE", IMM, IMM, PUSHDE, new List<int>{ 5, 3, 3 }) },

                { 0xD6, new Instruction("SUB A,n", IMM, IMM, SUBAN, new List<int>{ 4, 3 }) },
                { 0xDE, new Instruction("SBC A,n", IMM, IMM, SBCAN, new List<int>{ 4, 3 }) },

                { 0xE1, new Instruction("POP HL", IMM, IMM, POPHL, new List<int>{ 4, 3, 3 }) },
                { 0xE5, new Instruction("PUSH HL", IMM, IMM, PUSHHL, new List<int>{ 5, 3, 3 }) },
                { 0xF1, new Instruction("POP AF", IMM, IMM, POPAF, new List<int>{ 4, 3, 3 }) },
                { 0xF3, new Instruction("DI", IMP, IMP, DI, new List<int>{ 4 }) },
                { 0xF5, new Instruction("PUSH AF", IMM, IMM, PUSHAF, new List<int>{ 5, 3, 3 }) },

                { 0xF9, new Instruction("LD SP,HL", IMM, IMM, LDSPHL, new List<int>{ 6 }) },
                { 0xFB, new Instruction("EI", IMP, IMP, EI, new List<int>{ 4 }) },
                { 0xFE, new Instruction("CP n", IMM, IMP, CPN, new List<int>{ 4, 3 }) },

                // Multi-byte Opcode Prefixes
                { 0xCB, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0xDD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0xED, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0xFD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
            };

            _ddInstructions = new Dictionary<byte, Instruction>
            {
                { 0x21, new Instruction("LD IX,nn", IMM, IMP, LDIXNN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),IX", IMM, IDX, LDNNIX, new List<int>{ 4, 4, 3, 3, 3, 3 }) },

                { 0x2B, new Instruction("LD IX,(nn)", IMM, IDX, LDIXFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },

                { 0x34, new Instruction("INC (IX+d)", IMM, IDX, INCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("DEC (IX+d)", IMM, IDX, DECIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },

                { 0x36, new Instruction("LD (IX+d),n", IMM, IMM, LDIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x46, new Instruction("LD B,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x4E, new Instruction("LD C,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x56, new Instruction("LD D,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x5E, new Instruction("LD E,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x66, new Instruction("LD H,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x6E, new Instruction("LD L,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x70, new Instruction("LD (IX+d),B", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x71, new Instruction("LD (IX+d),C", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x72, new Instruction("LD (IX+d),D", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x73, new Instruction("LD (IX+d),E", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x74, new Instruction("LD (IX+d),H", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x75, new Instruction("LD (IX+d),L", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x77, new Instruction("LD (IX+d),A", IMM, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x7E, new Instruction("LD A,(IX+d)", IMM, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x86, new Instruction("ADD A,(IX+d)", IMM, IDX, ADDAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x8E, new Instruction("ADC A,(IX+d)", IMM, IDX, ADCAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x96, new Instruction("SUB A,(IX+d)", IMM, IDX, SUBAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x9E, new Instruction("SBC A,(IX+d)", IMM, IDX, SBCAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xBE, new Instruction("CP (IX+d)", IMM, IDX, CPIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xE1, new Instruction("POP IX", IMM, IMM, POPIX, new List<int>{ 4, 3, 3, 3 }) },
                { 0xE5, new Instruction("PUSH IX", IMM, IMM, PUSHIX, new List<int>{ 4, 5, 3, 3 }) },

                { 0xF9, new Instruction("LD SP,IX", IMM, IMM, LDSPIX, new List<int>{ 4, 6 }) },
            };

            _fdInstructions = new Dictionary<byte, Instruction>
            {
                { 0x21, new Instruction("LD IY,nn", IMM, IMP, LDIYNN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),IY", IMM, IDX, LDNNIY, new List<int>{ 4, 4, 3, 3, 3, 3 }) },

                { 0x2B, new Instruction("LD IY,(nn)", IMM, IDX, LDIYFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },

                { 0x34, new Instruction("INC (IY+d)", IMM, IDX, INCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("DEC (IY+d)", IMM, IDX, DECIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },

                { 0x36, new Instruction("LD (IY+d),n", IMM, IMM, LDIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x46, new Instruction("LD B,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x4E, new Instruction("LD C,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x56, new Instruction("LD D,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x5E, new Instruction("LD E,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x66, new Instruction("LD H,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x6E, new Instruction("LD L,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x70, new Instruction("LD (IY+d),B", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x71, new Instruction("LD (IY+d),C", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x72, new Instruction("LD (IY+d),D", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x73, new Instruction("LD (IY+d),E", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x74, new Instruction("LD (IY+d),H", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x75, new Instruction("LD (IY+d),L", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x77, new Instruction("LD (IY+d),A", IMM, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x7E, new Instruction("LD A,(IY+d)", IMM, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x86, new Instruction("ADD A,(IY+d)", IMM, IDX, ADDAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x8E, new Instruction("ADC A,(IY+d)", IMM, IDX, ADCAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x96, new Instruction("SUB A,(IY+d)", IMM, IDX, SUBAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x9E, new Instruction("SBC A,(IY+d)", IMM, IDX, SBCAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xBE, new Instruction("CP (IY+d)", IMM, IDX, CPIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xE1, new Instruction("POP IY", IMM, IMM, POPIY, new List<int>{ 4, 3, 3, 3 }) },
                { 0xE5, new Instruction("PUSH IY", IMM, IMM, PUSHIY, new List<int>{ 4, 5, 3, 3 }) },

                { 0xF9, new Instruction("LD SP,IY", IMM, IMM, LDSPIY, new List<int>{ 4, 6 }) },
            };

            _edInstructions = new Dictionary<byte, Instruction>
            {
                { 0x43, new Instruction("LD (nn),BC", IMM, IDX, LDNNBC, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x44, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x46, new Instruction("IM 0", IMP, IMP, IM0, new List<int>{ 4, 4 }) },
                { 0x47, new Instruction("ADD I,A", REG, REG, LDIA, new List<int>{ 4, 5 }) },
                { 0x4B, new Instruction("LD BC,(nn)", IMM, IDX, LDBCFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x53, new Instruction("LD (nn),DE", IMM, IDX, LDNNDE, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x56, new Instruction("IM 1", IMP, IMP, IM1, new List<int>{ 4, 4 }) },
                { 0x5B, new Instruction("LD DE,(nn)", IMM, IDX, LDDEFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x5E, new Instruction("IM 2", IMP, IMP, IM2, new List<int>{ 4, 4 }) },
                { 0x63, new Instruction("LD (nn),HL", IMM, IDX, LDNNHL2, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x6B, new Instruction("LD HL,(nn)", IMM, IDX, LDHLFNN2, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x73, new Instruction("LD (nn),SP", IMM, IDX, LDNNSP, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x7B, new Instruction("LD SP,(nn)", IMM, IDX, LDSPFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },

                { 0x57, new Instruction("ADD A,I", REG, REG, LDAI, new List<int>{ 4, 5 }) },
                { 0x4F, new Instruction("ADD R,A", REG, REG, LDRA, new List<int>{ 4, 5 }) },
                { 0x5F, new Instruction("ADD A,R", REG, REG, LDAR, new List<int>{ 4, 5 }) },
            };
        }

        public void ConnectToBus(IBus bus) => _bus = bus;

        public void Step()
        {
            var nextIns = PeekNextInstruction(ReadFromBus(PC));
            var tStates = nextIns.operation.TStates;

            for (int i = 0; i < tStates; i++)
            {
                Tick();
            }
        }

        private void Tick()
        {
            if (_clockCycles == 0)
            {
                _currentOpCode = ReadFromBus(PC);
                var nextIns = FetchNextInstruction(_currentOpCode);
                _currentOpCode = nextIns.opCode;
                _clockCycles = nextIns.operation.TStates;
                nextIns.operation.Op(_currentOpCode);
            }

            _clockCycles--;
        }

        private byte ReadFromBus(ushort addr) => _bus.Read(addr, false);
        private void WriteToBus(ushort addr, byte data) => _bus.Write(addr, data);
        private byte ReadFromBusPort(byte port) => _bus.ReadPeripheral(port);
        private void WriteToBusPort(byte port, byte data) => _bus.WritePeripheral(port, data);

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

        private byte Fetch1(Dictionary<byte, Instruction> lookupTable)
        {
            if ((lookupTable[_currentOpCode].AddressingMode1 != IMP) &&
                (lookupTable[_currentOpCode].AddressingMode1 != REG))
            {
                lookupTable[_currentOpCode].AddressingMode1();
                return ReadFromBus(_absoluteAddress);
            }

            return 0x00;
        }

        private byte Fetch2(Dictionary<byte, Instruction> lookupTable)
        {
            if ((lookupTable[_currentOpCode].AddressingMode2 != IMP) &&
                (lookupTable[_currentOpCode].AddressingMode2 != REG))
            {
                lookupTable[_currentOpCode].AddressingMode2();
                return ReadFromBus(_absoluteAddress);
            }

            return 0x00;
        }

        private (byte opCode, Instruction operation) PeekNextInstruction(byte code) => FetchNextInstruction(code, false);

        private (byte opCode, Instruction operation) FetchNextInstruction(byte code, bool incPC = true)
        {
            // ToDo: Need to handle interrupts to release CPU from HALT state!
            if (!_rootInstructions[code].Mnemonic.Equals("HALT", StringComparison.InvariantCultureIgnoreCase))
            {
                if (incPC) PC++;
            }

            switch (code)
            {
                case 0xCB:
                    var opCB = incPC ? ReadFromBus(PC) : ReadFromBus((ushort)(PC + 1));
                    if (incPC) PC++;
                    return (opCB, _cbInstructions[opCB]);
                case 0xDD:
                    var opDD = incPC ? ReadFromBus(PC) : ReadFromBus((ushort)(PC + 1));
                    if (incPC) PC++;
                    if (opDD == 0xCB)
                    {
                        var opDDCB = incPC ? ReadFromBus((ushort)(PC + 1)) : ReadFromBus((ushort)(PC + 2));
                        if (incPC) PC++;
                        return (opDDCB, _ddcbInstructions[opDDCB]);
                    }

                    return (opDD, _ddInstructions[opDD]);
                case 0xED:
                    var opED = incPC ? ReadFromBus(PC) : ReadFromBus((ushort)(PC + 1));
                    if (incPC) PC++;
                    return (opED, _edInstructions[opED]);
                case 0xFD:
                    var opFD = incPC ? ReadFromBus(PC) : ReadFromBus((ushort)(PC + 1));
                    if (incPC) PC++;
                    if (opFD == 0xCB)
                    {
                        var opFDCB = incPC ? ReadFromBus((ushort)(PC + 1)) : ReadFromBus((ushort)(PC + 2));
                        if (incPC) PC++;
                        return (opFDCB, _fdcbInstructions[opFDCB]);
                    }

                    return (opFD, _fdInstructions[opFD]);
                default:
                    return (code, _rootInstructions[code]);
            }
        }
    }
}
