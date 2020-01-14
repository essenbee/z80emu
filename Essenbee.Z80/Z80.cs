using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
        //` ![](E5D9CCCCCD8B82BB1159F822CFE086F8.png;;;0.03403,0.03945)
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
        public ushort AF => (ushort)(F + (A << 8));
        public ushort HL => (ushort)(L + (H << 8));
        public ushort BC => (ushort)((B << 8) + C);
        public ushort DE => (ushort)((D << 8) + E);

        // 16-bit Combined Shadow Registers
        public ushort AF1 => (ushort)(F1 + (A1 << 8));
        public ushort HL1 => (ushort)(L1 + (H1 << 8));
        public ushort BC1 => (ushort)((B1 << 8) + C1);
        public ushort DE1 => (ushort)((D1 << 8) + E1);

        // Interrupt Flip-flops
        public bool IFF1 { get; set; }
        public bool IFF2 { get; set; }
        public bool IsHalted { get; set; }
        public InterruptMode InterruptMode { get; set; } = InterruptMode.Mode0;
        public ushort MEMPTR { get; set; } = 0x0000; // aka WZ
        public Flags Q { get; set; } = 0x00; // See https://www.worldofspectrum.org/forums/discussion/41704/redirect/p1

        public long TotalTStates { get; set; }

        private IBus _bus = null!;

        public Dictionary<byte, Instruction> RootInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> CBInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> DDInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> DDCBInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> EDInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> FDInstructions { get; } = new Dictionary<byte, Instruction>();
        public Dictionary<byte, Instruction> FDCBInstructions { get; } = new Dictionary<byte, Instruction>();

        private ushort _absoluteAddress = 0x0000;
        private byte _currentOpCode = 0x00;
        private int _clockCycles = 0;

        public Z80()
        {
            RootInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0x01, new Instruction("LD BC,nn", IMX, IMP, LDBCNN, new List<int>{ 4, 3, 3 }) },
                { 0x02, new Instruction("LD (BC),A", IMP, IMP, LDBCA, new List<int>{ 4, 3 }) },
                { 0x03, new Instruction("INC BC", IMP, IMP, INCSS, new List<int>{ 6 }) },
                { 0x04, new Instruction("INC B", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x05, new Instruction("DEC B", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x06, new Instruction("LD B,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x07, new Instruction("RLCA", IMP, IMP, RLCA, new List<int>{ 4 }) },
                { 0x08, new Instruction("EX AF,AF'", IMP, IMP, EXAFAF, new List<int>{ 4 }) },
                { 0x09, new Instruction("ADD HL,BC", IMP, IMP, ADDHLSS, new List<int>{ 4, 4, 3 }) },
                { 0x0A, new Instruction("LD A,(BC)", IDX, IMP, LDABC, new List<int>{ 4, 3 }) },
                { 0x0B, new Instruction("DEC BC", IMP, IMP, DECSS, new List<int>{ 6 }) },
                { 0x0C, new Instruction("INC C", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x0D, new Instruction("DEC C", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x0E, new Instruction("LD C,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x0F, new Instruction("RRCA", IMP, IMP, RRCA, new List<int>{ 4 }) },
                { 0x10, new Instruction("DJNZ e", REL, IMP, DJNZ, new List<int> { 5, 3 } ) },
                { 0x11, new Instruction("LD DE,nn", IMX, IMP, LDDENN, new List<int>{ 4, 3, 3 }) },
                { 0x12, new Instruction("LD (DE),A", IMP, IMP, LDDEA, new List<int>{ 4, 3 }) },
                { 0x13, new Instruction("INC DE", IMP, IMP, INCSS, new List<int>{ 6 }) },
                { 0x14, new Instruction("INC D", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x15, new Instruction("DEC D", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x16, new Instruction("LD D,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x17, new Instruction("RLA", IMP, IMP, RLA, new List<int>{ 4 }) },
                { 0x18, new Instruction("JR e", REL, IMM, JR, new List<int>{ 4, 3, 5 }) },
                { 0x19, new Instruction("ADD HL,DE", IMP, IMP, ADDHLSS, new List<int>{ 4, 4, 3 }) },
                { 0x1A, new Instruction("LD A,(DE)", IDX, IMP, LDADE, new List<int>{ 4, 3 }) },
                { 0x1B, new Instruction("DEC DE", IMP, IMP, DECSS, new List<int>{ 6 }) },
                { 0x1C, new Instruction("INC E", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x1D, new Instruction("DEC E", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x1E, new Instruction("LD E,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x1F, new Instruction("RRA", IMP, IMP, RRA, new List<int>{ 4 }) },
                { 0x20, new Instruction("JR NZ,e", REL, IMP, JRNZ, new List<int>{ 4, 3 }) },
                { 0x21, new Instruction("LD HL,nn", IMX, IMP, LDHLNN, new List<int>{ 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),HL", IMX, IDX, LDNNHL, new List<int>{ 4, 3, 3, 3, 3 }) },
                { 0x23, new Instruction("INC HL", IMP, IMP, INCSS, new List<int>{ 6 }) },
                { 0x24, new Instruction("INC H", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x25, new Instruction("DEC H", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x26, new Instruction("LD H,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x27, new Instruction("DAA", IMP, IMP, DAA, new List<int>{ 4 }) },
                { 0x28, new Instruction("JR Z,e", REL, IMP, JRZ, new List<int>{ 4, 3 }) },
                { 0x29, new Instruction("ADD HL,HL", IMP, IMP, ADDHLSS, new List<int>{ 4, 4, 3 }) },
                { 0x2A, new Instruction("LD HL,(nn)", IMX, IDX, LDHLFNN, new List<int>{ 4, 3, 3, 3, 3 }) },
                { 0x2B, new Instruction("DEC HL", IMP, IMP, DECSS, new List<int>{ 6 }) },
                { 0x2C, new Instruction("INC L", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x2D, new Instruction("DEC L", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x2E, new Instruction("LD L,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
                { 0x2F, new Instruction("CPL", IMP, IMP, CPL, new List<int>{ 4 }) },
                { 0x30, new Instruction("JR C,e", REL, IMP, JRNC, new List<int>{ 4, 3 }) },
                { 0x31, new Instruction("LD SP,nn", IMX, IMP, LDSPNN, new List<int>{ 4, 3, 3 }) },
                { 0x32, new Instruction("LD (nn),A", IMX, IMP, LDNNA, new List<int>{ 4, 3, 3, 3 }) },
                { 0x33, new Instruction("INC SP", IMP, IMP, INCSS, new List<int>{ 6 }) },
                { 0x34, new Instruction("INC (HL)", RGIHL, IMP, INCHL, new List<int>{ 4, 4, 3 }) },
                { 0x35, new Instruction("DEC (HL)", RGIHL, IMP, DECHL, new List<int>{ 4, 4, 3 }) },
                { 0x36, new Instruction("LD (HL),n", IMM, IMP, LDHLN, new List<int>{ 4, 3, 3 }) },
                { 0x37, new Instruction("SCF", IMP, IMP, SCF, new List<int>{ 4 }) },
                { 0x38, new Instruction("JR C,e", REL, IMP, JRC, new List<int>{ 4, 3 }) },
                { 0x39, new Instruction("ADD HL,SP", IMP, IMP, ADDHLSS, new List<int>{ 4, 4, 3 }) },
                { 0x3A, new Instruction("LD A,(nn)", IMX, IDX, LDANN, new List<int>{ 4, 3, 3, 3 }) },
                { 0x3B, new Instruction("DEC SP", IMP, IMP, DECSS, new List<int>{ 6 }) },
                { 0x3C, new Instruction("INC A", IMP, IMP, INCR, new List<int>{ 4 }) },
                { 0x3D, new Instruction("DEC A", IMP, IMP, DECR, new List<int>{ 4 }) },
                { 0x3E, new Instruction("LD A,n", IMM, IMP, LDRN, new List<int>{ 4, 3 }) },
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
                { 0xA0, new Instruction("AND B", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA1, new Instruction("AND C", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA2, new Instruction("AND D", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA3, new Instruction("AND E", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA4, new Instruction("AND H", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA5, new Instruction("AND L", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA6, new Instruction("AND (HL)", RGIHL, RGIHL, ANDHL, new List<int>{ 4, 3 }) },
                { 0xA7, new Instruction("AND A", IMP, IMP, ANDR, new List<int>{ 4 }) },
                { 0xA8, new Instruction("XOR B", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xA9, new Instruction("XOR C", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xAA, new Instruction("XOR D", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xAB, new Instruction("XOR E", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xAC, new Instruction("XOR H", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xAD, new Instruction("XOR L", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xAE, new Instruction("XOR (HL)", RGIHL, RGIHL, XORHL, new List<int>{ 4, 3 }) },
                { 0xAF, new Instruction("XOR A", IMP, IMP, XORR, new List<int>{ 4 }) },
                { 0xB0, new Instruction("OR B", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB1, new Instruction("OR C", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB2, new Instruction("OR D", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB3, new Instruction("OR E", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB4, new Instruction("OR H", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB5, new Instruction("OR L", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB6, new Instruction("OR (HL)", RGIHL, RGIHL, ORHL, new List<int>{ 4, 3 }) },
                { 0xB7, new Instruction("OR A", IMP, IMP, ORR, new List<int>{ 4 }) },
                { 0xB8, new Instruction("CP B", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xB9, new Instruction("CP C", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBA, new Instruction("CP D", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBB, new Instruction("CP E", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBC, new Instruction("CP H", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBD, new Instruction("CP L", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xBE, new Instruction("BP (HL)", RGIHL, IMP, CPHL, new List<int>{ 4, 3 }) },
                { 0xBF, new Instruction("CP A", IMP, IMP, CPR, new List<int>{ 4 }) },
                { 0xC0, new Instruction("RET NZ", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xC1, new Instruction("POP BC", IMP, IMP, POPBC, new List<int>{ 4, 3, 3 }) },
                { 0xC2, new Instruction("JP NZ,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xC3, new Instruction("JP nn", IMX, IMP, JPNN, new List<int>{ 4, 3, 3 }) },
                { 0xC4, new Instruction("CALL NZ,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xC5, new Instruction("PUSH BC", IMP, IMP, PUSHBC, new List<int>{ 5, 3, 3 }) },
                { 0xC6, new Instruction("ADD A,n", IMM, IMP, ADDAN, new List<int>{ 4, 3 }) },
                { 0xC7, new Instruction("RST &00", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xC8, new Instruction("RET Z", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xC9, new Instruction("RET", IMP, IMP, RET, new List<int>{ 4, 3, 3 }) },
                { 0xCA, new Instruction("JP Z,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xCC, new Instruction("CALL Z,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xCD, new Instruction("CALL nn", IMX, IMP, CALL, new List<int>{ 4, 3, 4, 3, 3 }) },
                { 0xCE, new Instruction("ADC A,n", IMM, IMP, ADCAN, new List<int>{ 4, 3 }) },
                { 0xCF, new Instruction("RST &08", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xD0, new Instruction("RET NC", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xD1, new Instruction("POP DE", IMP, IMP, POPDE, new List<int>{ 4, 3, 3 }) },
                { 0xD2, new Instruction("JP NC,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xD3, new Instruction("OUT (n),A", IMM, IMP, OUTA, new List<int>{ 4, 3, 4 }) },
                { 0xD4, new Instruction("CALL NC,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xD5, new Instruction("PUSH DE", IMP, IMP, PUSHDE, new List<int>{ 5, 3, 3 }) },
                { 0xD6, new Instruction("SUB A,n", IMM, IMP, SUBAN, new List<int>{ 4, 3 }) },
                { 0xD7, new Instruction("RST &10", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xD8, new Instruction("RET Z", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xD9, new Instruction("EXX", IMP, IMP, EXX, new List<int>{ 4 }) },
                { 0xDA, new Instruction("JP C,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xDB, new Instruction("IN A,(n)", IMM, IMP, INA, new List<int>{ 4, 3, 4 }) },
                { 0xDC, new Instruction("CALL C,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xDE, new Instruction("SBC A,n", IMM, IMP, SBCAN, new List<int>{ 4, 3 }) },
                { 0xDF, new Instruction("RST &18", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xE0, new Instruction("RET PO", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xE1, new Instruction("POP HL", IMP, IMP, POPHL, new List<int>{ 4, 3, 3 }) },
                { 0xE2, new Instruction("JP PO,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xE3, new Instruction("EX (SP),HL", IMP, IMP, EXSPHL, new List<int>{ 4, 3, 4, 3, 5 }) },
                { 0xE4, new Instruction("CALL PO,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xE5, new Instruction("PUSH HL", IMP, IMP, PUSHHL, new List<int>{ 5, 3, 3 }) },
                { 0xE6, new Instruction("AND n", IMM, IMP, ANDN, new List<int>{ 4, 3 }) },
                { 0xE7, new Instruction("RST &20", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xE8, new Instruction("RET PE", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xE9, new Instruction("JP (HL)", IMP, IMP, JPHL, new List<int>{ 4 }) },
                { 0xEA, new Instruction("JP PE,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xEB, new Instruction("EX DE,HL", IMP, IMP, EXDEHL, new List<int>{ 4 }) },
                { 0xEC, new Instruction("CALL PE,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xEE, new Instruction("XOR n", IMM, IMP, XORN, new List<int>{ 4, 3 }) },
                { 0xEF, new Instruction("RST &28", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xF0, new Instruction("RET P", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xF1, new Instruction("POP AF", IMP, IMP, POPAF, new List<int>{ 4, 3, 3 }) },
                { 0xF2, new Instruction("JP P,nn", IMX, IMP, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xF3, new Instruction("DI", IMP, IMP, DI, new List<int>{ 4 }) },
                { 0xF4, new Instruction("CALL P,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xF5, new Instruction("PUSH AF", IMP, IMP, PUSHAF, new List<int>{ 5, 3, 3 }) },
                { 0xF6, new Instruction("OR n", IMM, IMP, ORN, new List<int>{ 4, 3 }) },
                { 0xF7, new Instruction("RST &30", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },
                { 0xF8, new Instruction("RET M", IMP, IMP, RETCC, new List<int>{ 5 }) },
                { 0xF9, new Instruction("LD SP,HL", IMP, IMP, LDSPHL, new List<int>{ 6 }) },
                { 0xFA, new Instruction("JP M,nn", IMX, IMM, JPCCNN, new List<int>{ 4, 3, 3 }) },
                { 0xFB, new Instruction("EI", IMP, IMP, EI, new List<int>{ 4 }) },
                { 0xFC, new Instruction("CALL M,nn", IMX, IMP, CALLCC, new List<int>{ 4, 3, 3 }) },
                { 0xFE, new Instruction("CP n", IMM, IMP, CPN, new List<int>{ 4, 3 }) },
                { 0xFF, new Instruction("RST &38", IMP, IMP, RST, new List<int>{ 5, 3, 3 }) },

                // In case of stray values in memory...
                { 0xDD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0xFD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
            };

            DDInstructions = new Dictionary<byte, Instruction>
            {
                { 0x09, new Instruction("ADD IX,BC", IMP, IMP, ADDIXPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x19, new Instruction("ADD IX,DE", IMP, IMP, ADDIXPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x21, new Instruction("LD IX,nn", IMX, IMP, LDIXNN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),IX", IMX, IDX, LDNNIX, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x23, new Instruction("INC IX", IMP, IMP, INCIX, new List<int>{ 4, 6 }) },
                { 0x29, new Instruction("ADD IX,IX", IMP, IMP, ADDIXPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x2A, new Instruction("LD IX,(nn)", IMX, IDX, LDIXFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x2B, new Instruction("DEC IX", IMP, IMP, DECIX, new List<int>{ 4, 6 }) },
                { 0x34, new Instruction("INC (IX+d)", REL, IDX, INCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("DEC (IX+d)", REL, IDX, DECIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },

                { 0x36, new Instruction("LD (IX+d),n", REL, IMM, LDIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x39, new Instruction("ADD IX,SP", IMP, IMP, ADDIXPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x46, new Instruction("LD B,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x4E, new Instruction("LD C,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x56, new Instruction("LD D,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x5E, new Instruction("LD E,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x66, new Instruction("LD H,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x6E, new Instruction("LD L,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x70, new Instruction("LD (IX+d),B", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x71, new Instruction("LD (IX+d),C", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x72, new Instruction("LD (IX+d),D", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x73, new Instruction("LD (IX+d),E", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x74, new Instruction("LD (IX+d),H", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x75, new Instruction("LD (IX+d),L", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x77, new Instruction("LD (IX+d),A", REL, IDX, LDIXDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x7E, new Instruction("LD A,(IX+d)", REL, IDX, LDRIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x86, new Instruction("ADD A,(IX+d)", REL, IDX, ADDAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x8E, new Instruction("ADC A,(IX+d)", REL, IDX, ADCAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x96, new Instruction("SUB A,(IX+d)", REL, IDX, SUBAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x9E, new Instruction("SBC A,(IX+d)", REL, IDX, SBCAIXDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xA6, new Instruction("AND (IX+d)", REL, IDX, ANDIXD, new List<int>{ 4, 4 , 3, 5, 3 }) },
                { 0xAE, new Instruction("XOR (IX+d)", REL, IDX, XORIXD, new List<int>{ 4, 4 , 3, 5, 3 }) },
                { 0xB6, new Instruction("OR (IX+d)", REL, IDX, ORIXD, new List<int>{ 4, 4 , 3, 5, 3 }) },

                { 0xBE, new Instruction("CP (IX+d)", REL, IDX, CPIXD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xDD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4, 4 }) },

                { 0xE1, new Instruction("POP IX", IMP, IMP, POPIX, new List<int>{ 4, 4, 3, 3 }) },
                { 0xE3, new Instruction("EX (SP),IX", IMP, IMP, EXSPIX, new List<int>{ 4, 4, 3, 4, 3, 5 }) },
                { 0xE5, new Instruction("PUSH IX", IMP, IMP, PUSHIX, new List<int>{ 4, 5, 3, 3 }) },
                { 0xE9, new Instruction("JP (IX)", IMP, IMP, JPIX, new List<int>{ 4, 4 }) },
                { 0xF9, new Instruction("LD SP,IX", IMP, IMP, LDSPIX, new List<int>{ 4, 6 }) },

                { 0xFD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4, 4 }) },
            };

            FDInstructions = new Dictionary<byte, Instruction>
            {
                { 0x09, new Instruction("ADD IY,BC", IMP, IMP, ADDIYPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x19, new Instruction("ADD IY,DE", IMP, IMP, ADDIYPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x21, new Instruction("LD IY,nn", IMX, IMP, LDIYNN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x22, new Instruction("LD (nn),IY", IMX, IDX, LDNNIY, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x23, new Instruction("INC IY", IMP, IMP, INCIY, new List<int>{ 4, 6 }) },
                { 0x29, new Instruction("ADD IY,IY", IMP, IMP, ADDIYPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x2A, new Instruction("LD IY,(nn)", IMX, IDX, LDIYFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x2B, new Instruction("DEC IY", IMP, IMP, DECIY, new List<int>{ 4, 6 }) },
                { 0x34, new Instruction("INC (IY+d)", REL, IDX, INCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("DEC (IY+d)", REL, IDX, DECIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },

                { 0x36, new Instruction("LD (IY+d),n", REL, IMM, LDIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x39, new Instruction("ADD IY,SP", IMP, IMP, ADDIYPP, new List<int>{ 4, 4, 4, 3 }) },
                { 0x46, new Instruction("LD B,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x4E, new Instruction("LD C,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x56, new Instruction("LD D,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x5E, new Instruction("LD E,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x66, new Instruction("LD H,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x6E, new Instruction("LD L,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x70, new Instruction("LD (IY+d),B", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x71, new Instruction("LD (IY+d),C", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x72, new Instruction("LD (IY+d),D", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x73, new Instruction("LD (IY+d),E", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x74, new Instruction("LD (IY+d),H", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x75, new Instruction("LD (IY+d),L", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x77, new Instruction("LD (IY+d),A", REL, IDX, LDIYDR, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x7E, new Instruction("LD A,(IY+d)", REL, IDX, LDRIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0x86, new Instruction("ADD A,(IY+d)", REL, IDX, ADDAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x8E, new Instruction("ADC A,(IY+d)", REL, IDX, ADCAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x96, new Instruction("SUB A,(IY+d)", REL, IDX, SUBAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },
                { 0x9E, new Instruction("SBC A,(IY+d)", REL, IDX, SBCAIYDN, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xA6, new Instruction("AND (IY+d)", REL, IDX, ANDIYD, new List<int>{ 4, 4 , 3, 5, 3 }) },
                { 0xAE, new Instruction("XOR (IY+d)", REL, IDX, XORIYD, new List<int>{ 4, 4 , 3, 5, 3 }) },
                { 0xB6, new Instruction("OR (IY+d)", REL, IDX, ORIYD, new List<int>{ 4, 4 , 3, 5, 3 }) },

                { 0xBE, new Instruction("CP (IY+d)", REL, IDX, CPIYD, new List<int>{ 4, 4, 3, 5, 3 }) },

                { 0xDD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4, 4 }) },

                { 0xE1, new Instruction("POP IY", IMP, IMP, POPIY, new List<int>{ 4, 4, 3, 3 }) },
                { 0xE3, new Instruction("EX (SP),IY", IMP, IMP, EXSPIY, new List<int>{ 4, 4, 3, 4, 3, 5 }) },
                { 0xE5, new Instruction("PUSH IY", IMP, IMP, PUSHIY, new List<int>{ 4, 5, 3, 3 }) },
                { 0xE9, new Instruction("JP (IY)", IMP, IMP, JPIY, new List<int>{ 4, 4 }) },
                { 0xF9, new Instruction("LD SP,IY", IMP, IMP, LDSPIY, new List<int>{ 4, 6 }) },

                { 0xFD, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4, 4 }) },
            };

            EDInstructions = new Dictionary<byte, Instruction>
            {
                { 0x40, new Instruction("IN B,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x41, new Instruction("OUT (C),B", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x42, new Instruction("SBC HL,BC", IMP, IMP, SBCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x43, new Instruction("LD (nn),BC", IMX, IDX, LDNNBC, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x44, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x45, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x46, new Instruction("IM 0", IMP, IMP, IM0, new List<int>{ 4, 4 }) },
                { 0x47, new Instruction("ADD I,A", REG, REG, LDIA, new List<int>{ 4, 5 }) },
                { 0x48, new Instruction("IN C,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x49, new Instruction("OUT (C),C", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x4A, new Instruction("ADC HL,BC", IMP, IMP, ADCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x4B, new Instruction("LD BC,(nn)", IMM, IDX, LDBCFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x4C, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x4D, new Instruction("RETI", IMP, IMP, RETI, new List<int>{ 4, 4, 3, 3 }) },
                { 0x4E, new Instruction("IM 0", IMP, IMP, IM0, new List<int>{ 4, 4 }) },
                { 0x4F, new Instruction("LD R,A", REG, REG, LDRA, new List<int>{ 4, 5 }) },
                { 0x50, new Instruction("IN D,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x51, new Instruction("OUT (C),D", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x52, new Instruction("SBC HL,DE", IMP, IMP, SBCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x53, new Instruction("LD (nn),DE", IMX, IDX, LDNNDE, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x54, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x55, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x56, new Instruction("IM 1", IMP, IMP, IM1, new List<int>{ 4, 4 }) },
                { 0x57, new Instruction("LD A,I", REG, REG, LDAI, new List<int>{ 4, 5 }) },
                { 0x58, new Instruction("IN D,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x59, new Instruction("OUT (C),E", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x5A, new Instruction("ADC HL,DE", IMP, IMP, ADCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x5B, new Instruction("LD DE,(nn)", IMX, IDX, LDDEFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x5C, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x5D, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x5E, new Instruction("IM 2", IMP, IMP, IM2, new List<int>{ 4, 4 }) },
                { 0x5F, new Instruction("LD A,R", REG, REG, LDAR, new List<int>{ 4, 5 }) },
                { 0x60, new Instruction("IN H,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x61, new Instruction("OUT (C),H", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x62, new Instruction("SBC HL,HL", IMP, IMP, SBCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x63, new Instruction("LD (nn),HL", IMX, IDX, LDNNHL2, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x64, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x65, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x66, new Instruction("IM 0", IMP, IMP, IM0, new List<int>{ 4, 4 }) },
                { 0x67, new Instruction("RRD", RGIHL, IMP, RRD, new List<int>{ 4, 4, 3, 4, 3 }) },
                { 0x68, new Instruction("IN L,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x69, new Instruction("OUT (C),L", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x6A, new Instruction("ADC HL,HL", IMP, IMP, ADCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x6B, new Instruction("LD HL,(nn)", IMX, IDX, LDHLFNN2, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x6C, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x6D, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x6E, new Instruction("IM 0", IMP, IMP, IM0, new List<int>{ 4, 4 }) },
                { 0x6F, new Instruction("RLD", RGIHL, IMP, RLD, new List<int>{ 4, 4, 3, 4, 3 }) },
                { 0x70, new Instruction("IN F,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x71, new Instruction("OUT (C),F", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x72, new Instruction("SBC HL,SP", IMP, IMP, SBCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x73, new Instruction("LD (nn),SP", IMX, IDX, LDNNSP, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x74, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x75, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x76, new Instruction("IM 1", IMP, IMP, IM1, new List<int>{ 4, 4 }) },
                { 0x77, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },
                { 0x78, new Instruction("IN A,(C)", IMP, IMP, INR, new List<int>{ 4, 4, 4 }) },
                { 0x79, new Instruction("OUT (C),A", IMP, IMP, OUTR, new List<int>{ 4, 4, 4 }) },
                { 0x7A, new Instruction("ADC HL,SP", IMP, IMP, ADCHLSS, new List<int>{ 4, 4, 4, 3 }) },
                { 0x7B, new Instruction("LD SP,(nn)", IMX, IDX, LDSPFNN, new List<int>{ 4, 4, 3, 3, 3, 3 }) },
                { 0x7C, new Instruction("NEG", IMP, IMP, NEG, new List<int>{ 4, 4 }) },
                { 0x7D, new Instruction("RETN", IMP, IMP, RETN, new List<int>{ 4, 4, 3, 3 }) },
                { 0x7E, new Instruction("IM 2", IMP, IMP, IM2, new List<int>{ 4, 4 }) },
                { 0x7F, new Instruction("NOP", IMP, IMP, NOP, new List<int>{ 4 }) },

                { 0xA0, new Instruction("LDI", IMP, IMP, LDI, new List<int>{ 4, 4, 3, 5 }) },
                { 0xA1, new Instruction("CPI", IMP, IMP, CPI, new List<int>{ 4, 4, 3, 5 }) },
                { 0xA2, new Instruction("INII", IMP, IMP, INI, new List<int>{ 4, 5, 3, 4 }) },
                { 0xA8, new Instruction("LDD", IMP, IMP, LDD, new List<int>{ 4, 4, 3, 5 }) },
                { 0xA9, new Instruction("CPD", IMP, IMP, CPD, new List<int>{ 4, 4, 3, 5 }) },

                { 0xB0, new Instruction("LDIR", IMP, IMP, LDIR, new List<int>{ 4, 4, 3, 5 }) },
                { 0xB1, new Instruction("CPIR", IMP, IMP, CPIR, new List<int>{ 4, 4, 3, 5 }) },
                { 0xB8, new Instruction("LDDR", IMP, IMP, LDDR, new List<int>{ 4, 4, 3, 5 }) },
                { 0xB9, new Instruction("CPDR", IMP, IMP, CPDR, new List<int>{ 4, 4, 3, 5 }) },
            };

            CBInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("RLC B", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x01, new Instruction("RLC C", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x02, new Instruction("RLC D", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x03, new Instruction("RLC E", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x04, new Instruction("RLC H", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x05, new Instruction("RLC L", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x06, new Instruction("RLC (HL)", RGIHL, IMP, RLCHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x07, new Instruction("RLC A", IMP, IMP, RLCR, new List<int>{ 4, 4 }) },
                { 0x08, new Instruction("RRC B", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x09, new Instruction("RRC C", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x0A, new Instruction("RRC D", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x0B, new Instruction("RRC E", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x0C, new Instruction("RRC H", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x0D, new Instruction("RRC L", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x0E, new Instruction("RRC (HL)", RGIHL, IMP, RRCHL, new List<int>{ 4, 4 , 4, 3}) },
                { 0x0F, new Instruction("RRC A", IMP, IMP, RRCR, new List<int>{ 4, 4 }) },
                { 0x10, new Instruction("RL B", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x11, new Instruction("RL C", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x12, new Instruction("RL D", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x13, new Instruction("RL E", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x14, new Instruction("RL H", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x15, new Instruction("RL L", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x16, new Instruction("RL (HL)", RGIHL, IMP, RLHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x17, new Instruction("RL A", IMP, IMP, RLR, new List<int>{ 4, 4 }) },
                { 0x18, new Instruction("RR B", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x19, new Instruction("RR C", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x1A, new Instruction("RR D", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x1B, new Instruction("RR E", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x1C, new Instruction("RR H", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x1D, new Instruction("RR L", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x1E, new Instruction("RR (HL)", RGIHL, IMP, RRHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x1F, new Instruction("RR A", IMP, IMP, RRR, new List<int>{ 4, 4 }) },
                { 0x20, new Instruction("SLA B", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x21, new Instruction("SLA C", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x22, new Instruction("SLA D", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x23, new Instruction("SLA E", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x24, new Instruction("SLA H", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x25, new Instruction("SLA L", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x26, new Instruction("SLA (HL)", RGIHL, IMP, SLAHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x27, new Instruction("SLA A", IMP, IMP, SLAR, new List<int>{ 4, 4 }) },
                { 0x28, new Instruction("SRA B", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x29, new Instruction("SRA C", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x2A, new Instruction("SRA D", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x2B, new Instruction("SRA E", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x2C, new Instruction("SRA H", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x2D, new Instruction("SRA L", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x2E, new Instruction("SRA (HL)", RGIHL, IMP, SRAHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x2F, new Instruction("SRA A", IMP, IMP, SRAR, new List<int>{ 4, 4 }) },
                { 0x30, new Instruction("SLS B", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x31, new Instruction("SLS C", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x32, new Instruction("SLS D", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x33, new Instruction("SLS E", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x34, new Instruction("SLS H", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x35, new Instruction("SLS L", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x36, new Instruction("SLS (HL)", RGIHL, IMP, SLSHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x37, new Instruction("SLS A", IMP, IMP, SLSR, new List<int>{ 4, 4 }) },
                { 0x38, new Instruction("SRL B", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x39, new Instruction("SRL C", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x3A, new Instruction("SRL D", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x3B, new Instruction("SRL E", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x3C, new Instruction("SRL H", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x3D, new Instruction("SRL L", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x3E, new Instruction("SRL (HL)", RGIHL, IMP, SRLHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x3F, new Instruction("SRL A", IMP, IMP, SRLR, new List<int>{ 4, 4 }) },
                { 0x40, new Instruction("BIT 0,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x41, new Instruction("BIT 0,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x42, new Instruction("BIT 0,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x43, new Instruction("BIT 0,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x44, new Instruction("BIT 0,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x45, new Instruction("BIT 0,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x46, new Instruction("BIT 0,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x47, new Instruction("BIT 0,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x48, new Instruction("BIT 1,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x49, new Instruction("BIT 1,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x4A, new Instruction("BIT 1,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x4B, new Instruction("BIT 1,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x4C, new Instruction("BIT 1,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x4D, new Instruction("BIT 1,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x4E, new Instruction("BIT 1,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x4F, new Instruction("BIT 1,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x50, new Instruction("BIT 2,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x51, new Instruction("BIT 2,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x52, new Instruction("BIT 2,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x53, new Instruction("BIT 2,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x54, new Instruction("BIT 2,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x55, new Instruction("BIT 2,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x56, new Instruction("BIT 2,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x57, new Instruction("BIT 2,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x58, new Instruction("BIT 3,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x59, new Instruction("BIT 3,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x5A, new Instruction("BIT 3,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x5B, new Instruction("BIT 3,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x5C, new Instruction("BIT 3,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x5D, new Instruction("BIT 3,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x5E, new Instruction("BIT 3,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x5F, new Instruction("BIT 3,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x60, new Instruction("BIT 4,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x61, new Instruction("BIT 4,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x62, new Instruction("BIT 4,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x63, new Instruction("BIT 4,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x64, new Instruction("BIT 4,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x65, new Instruction("BIT 4,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x66, new Instruction("BIT 4,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x67, new Instruction("BIT 4,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x68, new Instruction("BIT 5,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x69, new Instruction("BIT 5,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x6A, new Instruction("BIT 5,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x6B, new Instruction("BIT 5,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x6C, new Instruction("BIT 5,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x6D, new Instruction("BIT 5,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x6E, new Instruction("BIT 5,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x6F, new Instruction("BIT 5,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x70, new Instruction("BIT 6,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x71, new Instruction("BIT 6,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x72, new Instruction("BIT 6,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x73, new Instruction("BIT 6,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x74, new Instruction("BIT 6,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x75, new Instruction("BIT 6,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x76, new Instruction("BIT 6,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x77, new Instruction("BIT 6,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x78, new Instruction("BIT 7,B", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x79, new Instruction("BIT 7,C", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x7A, new Instruction("BIT 7,D", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x7B, new Instruction("BIT 7,E", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x7C, new Instruction("BIT 7,H", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x7D, new Instruction("BIT 7,L", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x7E, new Instruction("BIT 7,(HL)", RGIHL, IMP, BITHL, new List<int>{ 4, 4, 4 }) },
                { 0x7F, new Instruction("BIT 7,A", IMP, IMP, BITBR, new List<int>{ 4, 4 }) },
                { 0x80, new Instruction("RES 0,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x81, new Instruction("RES 0,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x82, new Instruction("RES 0,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x83, new Instruction("RES 0,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x84, new Instruction("RES 0,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x85, new Instruction("RES 0,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x86, new Instruction("RES 0,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x87, new Instruction("RES 0,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x88, new Instruction("RES 1,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x89, new Instruction("RES 1,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x8A, new Instruction("RES 1,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x8B, new Instruction("RES 1,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x8C, new Instruction("RES 1,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x8D, new Instruction("RES 1,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x8E, new Instruction("RES 1,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x8F, new Instruction("RES 1,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x90, new Instruction("RES 2,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x91, new Instruction("RES 2,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x92, new Instruction("RES 2,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x93, new Instruction("RES 2,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x94, new Instruction("RES 2,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x95, new Instruction("RES 2,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x96, new Instruction("RES 2,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x97, new Instruction("RES 2,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x98, new Instruction("RES 3,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x99, new Instruction("RES 3,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x9A, new Instruction("RES 3,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x9B, new Instruction("RES 3,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x9C, new Instruction("RES 3,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x9D, new Instruction("RES 3,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0x9E, new Instruction("RES 3,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0x9F, new Instruction("RES 3,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA0, new Instruction("RES 4,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA1, new Instruction("RES 4,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA2, new Instruction("RES 4,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA3, new Instruction("RES 4,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA4, new Instruction("RES 4,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA5, new Instruction("RES 4,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA6, new Instruction("RES 4,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xA7, new Instruction("RES 4,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA8, new Instruction("RES 5,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xA9, new Instruction("RES 5,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xAA, new Instruction("RES 5,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xAB, new Instruction("RES 5,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xAC, new Instruction("RES 5,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xAD, new Instruction("RES 5,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xAE, new Instruction("RES 5,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xAF, new Instruction("RES 5,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB0, new Instruction("RES 6,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB1, new Instruction("RES 6,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB2, new Instruction("RES 6,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB3, new Instruction("RES 6,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB4, new Instruction("RES 6,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB5, new Instruction("RES 6,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB6, new Instruction("RES 6,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xB7, new Instruction("RES 6,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB8, new Instruction("RES 7,B", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xB9, new Instruction("RES 7,C", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xBA, new Instruction("RES 7,D", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xBB, new Instruction("RES 7,E", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xBC, new Instruction("RES 7,H", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xBD, new Instruction("RES 7,L", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xBE, new Instruction("RES 7,(HL)", RGIHL, IMP, RESHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xBF, new Instruction("RES 7,A", IMP, IMP, RESBR, new List<int>{ 4, 4 }) },
                { 0xC0, new Instruction("SET 0,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC1, new Instruction("SET 0,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC2, new Instruction("SET 0,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC3, new Instruction("SET 0,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC4, new Instruction("SET 0,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC5, new Instruction("SET 0,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC6, new Instruction("SET 0,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xC7, new Instruction("SET 0,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC8, new Instruction("SET 1,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xC9, new Instruction("SET 1,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xCA, new Instruction("SET 1,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xCB, new Instruction("SET 1,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xCC, new Instruction("SET 1,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xCD, new Instruction("SET 1,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xCE, new Instruction("SET 1,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xCF, new Instruction("SET 1,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD0, new Instruction("SET 2,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD1, new Instruction("SET 2,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD2, new Instruction("SET 2,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD3, new Instruction("SET 2,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD4, new Instruction("SET 2,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD5, new Instruction("SET 2,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD6, new Instruction("SET 2,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xD7, new Instruction("SET 2,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD8, new Instruction("SET 3,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xD9, new Instruction("SET 3,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xDA, new Instruction("SET 3,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xDB, new Instruction("SET 3,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xDC, new Instruction("SET 3,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xDD, new Instruction("SET 3,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xDE, new Instruction("SET 3,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xDF, new Instruction("SET 3,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE0, new Instruction("SET 4,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE1, new Instruction("SET 4,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE2, new Instruction("SET 4,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE3, new Instruction("SET 4,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE4, new Instruction("SET 4,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE5, new Instruction("SET 4,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE6, new Instruction("SET 4,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xE7, new Instruction("SET 4,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE8, new Instruction("SET 5,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xE9, new Instruction("SET 5,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xEA, new Instruction("SET 5,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xEB, new Instruction("SET 5,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xEC, new Instruction("SET 5,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xED, new Instruction("SET 5,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xEE, new Instruction("SET 5,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xEF, new Instruction("SET 5,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF0, new Instruction("SET 6,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF1, new Instruction("SET 6,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF2, new Instruction("SET 6,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF3, new Instruction("SET 6,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF4, new Instruction("SET 6,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF5, new Instruction("SET 6,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF6, new Instruction("SET 6,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xF7, new Instruction("SET 6,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF8, new Instruction("SET 7,B", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xF9, new Instruction("SET 7,C", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xFA, new Instruction("SET 7,D", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xFB, new Instruction("SET 7,E", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xFC, new Instruction("SET 7,H", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xFD, new Instruction("SET 7,L", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
                { 0xFE, new Instruction("SET 7,(HL)", RGIHL, IMP, SETHL, new List<int>{ 4, 4, 4, 3 }) },
                { 0xFF, new Instruction("SET 7,A", IMP, IMP, SETBR, new List<int>{ 4, 4 }) },
            };

            // Note: DDCB operations are in the form DDCB{displacement}{opCode} - use RELS, IDX addressing mode
            DDCBInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("RLC (IX+d),B", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x01, new Instruction("RLC (IX+d),C", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x02, new Instruction("RLC (IX+d),D", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x03, new Instruction("RLC (IX+d),E", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x04, new Instruction("RLC (IX+d),H", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x05, new Instruction("RLC (IX+d),L", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x06, new Instruction("RLC (IX+d)", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x07, new Instruction("RLC (IX+d),A", RELS, IDX, RLCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x08, new Instruction("RRC (IX+d),B", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x09, new Instruction("RRC (IX+d),C", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0A, new Instruction("RRC (IX+d),D", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0B, new Instruction("RRC (IX+d),E", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0C, new Instruction("RRC (IX+d),H", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0D, new Instruction("RRC (IX+d),L", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0E, new Instruction("RRC (IX+d)", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0F, new Instruction("RRC (IX+d),A", RELS, IDX, RRCIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x10, new Instruction("RL (IX+d),B", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x11, new Instruction("RL (IX+d),C", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x12, new Instruction("RL (IX+d),D", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x13, new Instruction("RL (IX+d),E", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x14, new Instruction("RL (IX+d),H", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x15, new Instruction("RL (IX+d),L", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x16, new Instruction("RL (IX+d)", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x17, new Instruction("RL (IX+d),A", RELS, IDX, RLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x18, new Instruction("RR (IX+d),B", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x19, new Instruction("RR (IX+d),C", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1A, new Instruction("RR (IX+d),D", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1B, new Instruction("RR (IX+d),E", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1C, new Instruction("RR (IX+d),H", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1D, new Instruction("RR (IX+d),L", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1E, new Instruction("RR (IX+d)", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1F, new Instruction("RR (IX+d),A", RELS, IDX, RRIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x20, new Instruction("SLA (IX+d),B", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x21, new Instruction("SLA (IX+d),C", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x22, new Instruction("SLA (IX+d),D", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x23, new Instruction("SLA (IX+d),E", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x24, new Instruction("SLA (IX+d),H", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x25, new Instruction("SLA (IX+d),L", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x26, new Instruction("SLA (IX+d)", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x27, new Instruction("SLA (IX+d),A", RELS, IDX, SLAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x28, new Instruction("SRA (IX+d),B", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x29, new Instruction("SRA (IX+d),C", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2A, new Instruction("SRA (IX+d),D", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2B, new Instruction("SRA (IX+d),E", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2C, new Instruction("SRA (IX+d),H", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2D, new Instruction("SRA (IX+d),L", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2E, new Instruction("SRA (IX+d)", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2F, new Instruction("SRA (IX+d),A", RELS, IDX, SRAIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x30, new Instruction("SLS (IX+d),B", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x31, new Instruction("SLS (IX+d),C", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x32, new Instruction("SLS (IX+d),D", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x33, new Instruction("SLS (IX+d),E", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x34, new Instruction("SLS (IX+d),H", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("SLS (IX+d),L", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x36, new Instruction("SLS (IX+d)", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x37, new Instruction("SLS (IX+d),A", RELS, IDX, SLSIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x38, new Instruction("SRL (IX+d),B", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x39, new Instruction("SRL (IX+d),C", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3A, new Instruction("SRL (IX+d),D", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3B, new Instruction("SRL (IX+d),E", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3C, new Instruction("SRL (IX+d),H", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3D, new Instruction("SRL (IX+d),L", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3E, new Instruction("SRL (IX+d)", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3F, new Instruction("SRL (IX+d),A", RELS, IDX, SRLIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x40, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x41, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x42, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x43, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x44, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x45, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x46, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x47, new Instruction("BIT 0,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x48, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x49, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4A, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4B, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4C, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4D, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4E, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4F, new Instruction("BIT 1,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x50, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x51, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x52, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x53, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x54, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x55, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x56, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x57, new Instruction("BIT 2,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x58, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x59, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5A, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5B, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5C, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5D, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5E, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5F, new Instruction("BIT 3,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x60, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x61, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x62, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x63, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x64, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x65, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x66, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x67, new Instruction("BIT 4,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x68, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x69, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6A, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6B, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6C, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6D, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6E, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6F, new Instruction("BIT 5,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x70, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x71, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x72, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x73, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x74, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x75, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x76, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x77, new Instruction("BIT 6,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x78, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x79, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7A, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7B, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7C, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7D, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7E, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7F, new Instruction("BIT 7,(IX+d)", RELS, IDX, BITIXD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x80, new Instruction("RES 0,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x81, new Instruction("RES 0,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x82, new Instruction("RES 0,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x83, new Instruction("RES 0,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x84, new Instruction("RES 0,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x85, new Instruction("RES 0,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x86, new Instruction("RES 0,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x87, new Instruction("RES 0,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x88, new Instruction("RES 1,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x89, new Instruction("RES 1,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8A, new Instruction("RES 1,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8B, new Instruction("RES 1,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8C, new Instruction("RES 1,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8D, new Instruction("RES 1,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8E, new Instruction("RES 1,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8F, new Instruction("RES 1,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x90, new Instruction("RES 2,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x91, new Instruction("RES 2,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x92, new Instruction("RES 2,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x93, new Instruction("RES 2,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x94, new Instruction("RES 2,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x95, new Instruction("RES 2,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x96, new Instruction("RES 2,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x97, new Instruction("RES 2,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x98, new Instruction("RES 3,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x99, new Instruction("RES 3,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9A, new Instruction("RES 3,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9B, new Instruction("RES 3,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9C, new Instruction("RES 3,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9D, new Instruction("RES 3,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9E, new Instruction("RES 3,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9F, new Instruction("RES 3,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA0, new Instruction("RES 4,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA1, new Instruction("RES 4,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA2, new Instruction("RES 4,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA3, new Instruction("RES 4,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA4, new Instruction("RES 4,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA5, new Instruction("RES 4,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA6, new Instruction("RES 4,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA7, new Instruction("RES 4,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA8, new Instruction("RES 5,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA9, new Instruction("RES 5,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAA, new Instruction("RES 5,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAB, new Instruction("RES 5,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAC, new Instruction("RES 5,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAD, new Instruction("RES 5,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAE, new Instruction("RES 5,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAF, new Instruction("RES 5,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB0, new Instruction("RES 6,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB1, new Instruction("RES 6,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB2, new Instruction("RES 6,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB3, new Instruction("RES 6,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB4, new Instruction("RES 6,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB5, new Instruction("RES 6,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB6, new Instruction("RES 6,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB7, new Instruction("RES 6,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB8, new Instruction("RES 7,(IX+d),B", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB9, new Instruction("RES 7,(IX+d),C", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBA, new Instruction("RES 7,(IX+d),D", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBB, new Instruction("RES 7,(IX+d),E", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBC, new Instruction("RES 7,(IX+d),H", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBD, new Instruction("RES 7,(IX+d),L", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBE, new Instruction("RES 7,(IX+d)", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBF, new Instruction("RES 7,(IX+d),A", RELS, IDX, RESIXD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xC0, new Instruction("SET 0,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC1, new Instruction("SET 0,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC2, new Instruction("SET 0,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC3, new Instruction("SET 0,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC4, new Instruction("SET 0,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC5, new Instruction("SET 0,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC6, new Instruction("SET 0,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC7, new Instruction("SET 0,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC8, new Instruction("SET 1,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC9, new Instruction("SET 1,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCA, new Instruction("SET 1,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCB, new Instruction("SET 1,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCC, new Instruction("SET 1,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCD, new Instruction("SET 1,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCE, new Instruction("SET 1,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCF, new Instruction("SET 1,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD0, new Instruction("SET 2,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD1, new Instruction("SET 2,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD2, new Instruction("SET 2,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD3, new Instruction("SET 2,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD4, new Instruction("SET 2,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD5, new Instruction("SET 2,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD6, new Instruction("SET 2,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD7, new Instruction("SET 2,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD8, new Instruction("SET 3,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD9, new Instruction("SET 3,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDA, new Instruction("SET 3,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDB, new Instruction("SET 3,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDC, new Instruction("SET 3,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDD, new Instruction("SET 3,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDE, new Instruction("SET 3,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDF, new Instruction("SET 3,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE0, new Instruction("SET 4,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE1, new Instruction("SET 4,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE2, new Instruction("SET 4,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE3, new Instruction("SET 4,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE4, new Instruction("SET 4,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE5, new Instruction("SET 4,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE6, new Instruction("SET 4,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE7, new Instruction("SET 4,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE8, new Instruction("SET 5,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE9, new Instruction("SET 5,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEA, new Instruction("SET 5,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEB, new Instruction("SET 5,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEC, new Instruction("SET 5,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xED, new Instruction("SET 5,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEE, new Instruction("SET 5,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEF, new Instruction("SET 5,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF0, new Instruction("SET 6,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF1, new Instruction("SET 6,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF2, new Instruction("SET 6,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF3, new Instruction("SET 6,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF4, new Instruction("SET 6,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF5, new Instruction("SET 6,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF6, new Instruction("SET 6,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF7, new Instruction("SET 6,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF8, new Instruction("SET 7,(IX+d),B", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF9, new Instruction("SET 7,(IX+d),C", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFA, new Instruction("SET 7,(IX+d),D", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFB, new Instruction("SET 7,(IX+d),E", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFC, new Instruction("SET 7,(IX+d),H", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFD, new Instruction("SET 7,(IX+d),L", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFE, new Instruction("SET 7,(IX+d)", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFF, new Instruction("SET 7,(IX+d),A", RELS, IDX, SETIXD, new List<int>{ 4, 4, 3,5,4,3 }) },
            };

            // Note: FDCB operations are in the form FDCB{displacement}{opCode} - use RELS, IDX addressing mode
            FDCBInstructions = new Dictionary<byte, Instruction>
            {
                { 0x00, new Instruction("RLC (IY+d),B", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x01, new Instruction("RLC (IY+d),C", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x02, new Instruction("RLC (IY+d),D", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x03, new Instruction("RLC (IY+d),E", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x04, new Instruction("RLC (IY+d),H", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x05, new Instruction("RLC (IY+d),L", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x06, new Instruction("RLC (IY+d)", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x07, new Instruction("RLC (IY+d),A", RELS, IDX, RLCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x08, new Instruction("RRC (IY+d),B", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x09, new Instruction("RRC (IY+d),C", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0A, new Instruction("RRC (IY+d),D", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0B, new Instruction("RRC (IY+d),E", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0C, new Instruction("RRC (IY+d),H", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0D, new Instruction("RRC (IY+d),L", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0E, new Instruction("RRC (IY+d)", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x0F, new Instruction("RRC (IY+d),A", RELS, IDX, RRCIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x10, new Instruction("RL (IY+d),B", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x11, new Instruction("RL (IY+d),C", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x12, new Instruction("RL (IY+d),D", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x13, new Instruction("RL (IY+d),E", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x14, new Instruction("RL (IY+d),H", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x15, new Instruction("RL (IY+d),L", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x16, new Instruction("RL (IY+d)", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x17, new Instruction("RL (IY+d),A", RELS, IDX, RLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x18, new Instruction("RR (IY+d),B", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x19, new Instruction("RR (IY+d),C", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1A, new Instruction("RR (IY+d),D", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1B, new Instruction("RR (IY+d),E", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1C, new Instruction("RR (IY+d),H", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1D, new Instruction("RR (IY+d),L", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1E, new Instruction("RR (IY+d)", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x1F, new Instruction("RR (IY+d),A", RELS, IDX, RRIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x20, new Instruction("SLA (IY+d),B", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x21, new Instruction("SLA (IY+d),C", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x22, new Instruction("SLA (IY+d),D", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x23, new Instruction("SLA (IY+d),E", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x24, new Instruction("SLA (IY+d),H", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x25, new Instruction("SLA (IY+d),L", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x26, new Instruction("SLA (IY+d)", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x27, new Instruction("SLA (IY+d),A", RELS, IDX, SLAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x28, new Instruction("SRA (IY+d),B", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x29, new Instruction("SRA (IY+d),C", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2A, new Instruction("SRA (IY+d),D", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2B, new Instruction("SRA (IY+d),E", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2C, new Instruction("SRA (IY+d),H", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2D, new Instruction("SRA (IY+d),L", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2E, new Instruction("SRA (IY+d)", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x2F, new Instruction("SRA (IY+d),A", RELS, IDX, SRAIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x30, new Instruction("SLS (IY+d),B", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x31, new Instruction("SLS (IY+d),C", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x32, new Instruction("SLS (IY+d),D", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x33, new Instruction("SLS (IY+d),E", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x34, new Instruction("SLS (IY+d),H", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x35, new Instruction("SLS (IY+d),L", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x36, new Instruction("SLS (IY+d)", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x37, new Instruction("SLS (IY+d),A", RELS, IDX, SLSIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x38, new Instruction("SRL (IY+d),B", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x39, new Instruction("SRL (IY+d),C", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3A, new Instruction("SRL (IY+d),D", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3B, new Instruction("SRL (IY+d),E", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3C, new Instruction("SRL (IY+d),H", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3D, new Instruction("SRL (IY+d),L", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3E, new Instruction("SRL (IY+d)", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x3F, new Instruction("SRL (IY+d),A", RELS, IDX, SRLIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x40, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x41, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x42, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x43, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x44, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x45, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x46, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x47, new Instruction("BIT 0,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x48, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x49, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4A, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4B, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4C, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4D, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4E, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x4F, new Instruction("BIT 1,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                   
                { 0x50, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x51, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x52, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x53, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x54, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x55, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x56, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x57, new Instruction("BIT 2,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                      
                { 0x58, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x59, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5A, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5B, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5C, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5D, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5E, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x5F, new Instruction("BIT 3,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                      
                { 0x60, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x61, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x62, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x63, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x64, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x65, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x66, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x67, new Instruction("BIT 4,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                       
                { 0x68, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x69, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6A, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6B, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6C, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6D, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6E, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x6F, new Instruction("BIT 5,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                       
                { 0x70, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x71, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x72, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x73, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x74, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x75, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x76, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x77, new Instruction("BIT 6,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },                                                                       
                { 0x78, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x79, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7A, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7B, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7C, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7D, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7E, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x7F, new Instruction("BIT 7,(IY+d)", RELS, IDX, BITIYD, new List<int>{ 4, 4, 3, 5, 4 }) },
                { 0x80, new Instruction("RES 0,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x81, new Instruction("RES 0,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x82, new Instruction("RES 0,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x83, new Instruction("RES 0,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x84, new Instruction("RES 0,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x85, new Instruction("RES 0,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x86, new Instruction("RES 0,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x87, new Instruction("RES 0,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x88, new Instruction("RES 1,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x89, new Instruction("RES 1,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8A, new Instruction("RES 1,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8B, new Instruction("RES 1,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8C, new Instruction("RES 1,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8D, new Instruction("RES 1,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8E, new Instruction("RES 1,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x8F, new Instruction("RES 1,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x90, new Instruction("RES 2,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x91, new Instruction("RES 2,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x92, new Instruction("RES 2,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x93, new Instruction("RES 2,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x94, new Instruction("RES 2,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x95, new Instruction("RES 2,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x96, new Instruction("RES 2,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x97, new Instruction("RES 2,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x98, new Instruction("RES 3,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x99, new Instruction("RES 3,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9A, new Instruction("RES 3,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9B, new Instruction("RES 3,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9C, new Instruction("RES 3,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9D, new Instruction("RES 3,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9E, new Instruction("RES 3,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0x9F, new Instruction("RES 3,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA0, new Instruction("RES 4,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA1, new Instruction("RES 4,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA2, new Instruction("RES 4,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA3, new Instruction("RES 4,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA4, new Instruction("RES 4,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA5, new Instruction("RES 4,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA6, new Instruction("RES 4,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA7, new Instruction("RES 4,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA8, new Instruction("RES 5,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xA9, new Instruction("RES 5,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAA, new Instruction("RES 5,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAB, new Instruction("RES 5,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAC, new Instruction("RES 5,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAD, new Instruction("RES 5,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAE, new Instruction("RES 5,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xAF, new Instruction("RES 5,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB0, new Instruction("RES 6,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB1, new Instruction("RES 6,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB2, new Instruction("RES 6,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB3, new Instruction("RES 6,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB4, new Instruction("RES 6,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB5, new Instruction("RES 6,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB6, new Instruction("RES 6,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB7, new Instruction("RES 6,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB8, new Instruction("RES 7,(IY+d),B", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xB9, new Instruction("RES 7,(IY+d),C", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBA, new Instruction("RES 7,(IY+d),D", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBB, new Instruction("RES 7,(IY+d),E", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBC, new Instruction("RES 7,(IY+d),H", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBD, new Instruction("RES 7,(IY+d),L", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBE, new Instruction("RES 7,(IY+d)", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xBF, new Instruction("RES 7,(IY+d),A", RELS, IDX, RESIYD, new List<int>{ 4, 4, 3, 5, 4, 3 }) },
                { 0xC0, new Instruction("SET 0,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC1, new Instruction("SET 0,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC2, new Instruction("SET 0,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC3, new Instruction("SET 0,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC4, new Instruction("SET 0,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC5, new Instruction("SET 0,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC6, new Instruction("SET 0,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC7, new Instruction("SET 0,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC8, new Instruction("SET 1,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xC9, new Instruction("SET 1,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCA, new Instruction("SET 1,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCB, new Instruction("SET 1,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCC, new Instruction("SET 1,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCD, new Instruction("SET 1,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCE, new Instruction("SET 1,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xCF, new Instruction("SET 1,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD0, new Instruction("SET 2,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD1, new Instruction("SET 2,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD2, new Instruction("SET 2,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD3, new Instruction("SET 2,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD4, new Instruction("SET 2,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD5, new Instruction("SET 2,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD6, new Instruction("SET 2,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD7, new Instruction("SET 2,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD8, new Instruction("SET 3,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xD9, new Instruction("SET 3,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDA, new Instruction("SET 3,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDB, new Instruction("SET 3,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDC, new Instruction("SET 3,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDD, new Instruction("SET 3,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDE, new Instruction("SET 3,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xDF, new Instruction("SET 3,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE0, new Instruction("SET 4,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE1, new Instruction("SET 4,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE2, new Instruction("SET 4,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE3, new Instruction("SET 4,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE4, new Instruction("SET 4,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE5, new Instruction("SET 4,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE6, new Instruction("SET 4,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE7, new Instruction("SET 4,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE8, new Instruction("SET 5,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xE9, new Instruction("SET 5,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEA, new Instruction("SET 5,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEB, new Instruction("SET 5,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEC, new Instruction("SET 5,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xED, new Instruction("SET 5,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEE, new Instruction("SET 5,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xEF, new Instruction("SET 5,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF0, new Instruction("SET 6,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF1, new Instruction("SET 6,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF2, new Instruction("SET 6,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF3, new Instruction("SET 6,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF4, new Instruction("SET 6,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF5, new Instruction("SET 6,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF6, new Instruction("SET 6,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF7, new Instruction("SET 6,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF8, new Instruction("SET 7,(IY+d),B", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xF9, new Instruction("SET 7,(IY+d),C", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFA, new Instruction("SET 7,(IY+d),D", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFB, new Instruction("SET 7,(IY+d),E", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFC, new Instruction("SET 7,(IY+d),H", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFD, new Instruction("SET 7,(IY+d),L", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFE, new Instruction("SET 7,(IY+d)", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
                { 0xFF, new Instruction("SET 7,(IY+d),A", RELS, IDX, SETIYD, new List<int>{ 4, 4, 3,5,4,3 }) },
            };
        }

        public void ConnectToBus(IBus bus) => _bus = bus;

        public void Reset(bool hardReset = false)
        {
            _clockCycles = 0;
            A = A1 = 0xFF;
            F = F1 = (Flags)0b1111_1111;
            I = R = 0x00;
            PC = 0x0000;
            SP = 0xFFFF;
            IFF1 = IFF2 = false;
            InterruptMode = InterruptMode.Mode0;
            IsHalted = false;
            Q = 0x00;

            if (hardReset)
            {
                B = C = D = E = H = L = 0x00;
                B1 = C1 = D1 = E1 = H1 = L1 = 0x00;
                IX = IY = 0x00;
                MEMPTR = 0x0000;
                TotalTStates = 0;
            }
        }

        public void Run()
        {
            do
            {
                Step();

            } while (true);
        }

        public void RunTo(byte opCode)
        {
            byte code;

            do
            {
                Step();
                var address = PC;
                (code, _, _) = PeekNextInstruction(ReadFromBus(address), ref address);
            } while (code != opCode);
        }

        public void Step()
        {
            var address = PC;
            var (_, operation, _) = PeekNextInstruction(ReadFromBus(address), ref address);
            var tStates = operation.TStates;

            for (int i = 0; i < tStates; i++)
            {
                Tick();
            }

            HandleInterrupts();
        }

        private void HandleInterrupts()
        {
            if (_bus.NonMaskableInterrupt)
            {
                NonMaskableInterrupt();
            }

            if (_bus.Interrupt)
            {
                Interrupt();
            }
        }

        public void Interrupt()
        {
            if (IFF1)
            {
                IFF1 = IFF2 = false;
                UnhaltIfHalted();

                switch (InterruptMode)
                {
                    case InterruptMode.Mode0:
                        // Read instruction from interrupting device
                        var instruction = _bus.Data;

                        if (instruction.Count > 0)
                        {
                            // If RST, push the PC onto the stack
                            if (RootInstructions[instruction[0]].Mnemonic
                                .StartsWith("RST", StringComparison.InvariantCultureIgnoreCase))
                            {
                                PushProgramCounter();
                                var addr = (ushort)(instruction[0] * 0x38);
                                PC = addr;
                                MEMPTR = addr;
                                ResetQ();

                                // ToDo: wait 13 T-states
                            }
                            else if (RootInstructions[instruction[0]].Mnemonic
                                     .StartsWith("CALL", StringComparison.InvariantCultureIgnoreCase) &&
                                     instruction.Count == 3)
                            {
                                PushProgramCounter();
                                // Little-endian
                                var addr = (ushort)((instruction[2] << 8) + instruction[1]);
                                PC = addr;
                                MEMPTR = addr;
                                ResetQ();

                                // ToDo: wait 13 T-states
                            }
                            else
                            {
                                // ToDo: Currently only RST and CALL supported
                            }
                        }
                        break;
                    case InterruptMode.Mode1:
                        PushProgramCounter();
                        PC = 0x0038; // There must be a handling routine here!

                        // ToDo: wait 13 T-states
                        break;
                    case InterruptMode.Mode2:
                        if (_bus.Data != null && _bus.Data.Count == 1)
                        {
                            PushProgramCounter();
                            var vector = _bus.Data[0];
                            PushProgramCounter();
                            var addr = (ushort)((I << 8) + vector);

                            //Get starting address from vector table (little-endian)
                            var loByte = ReadFromBus(addr++);
                            var hiByte = ReadFromBus(addr);
                            PC = (ushort)((hiByte << 8) + loByte);
                        }

                        // ToDo: wait 19 T-states

                        break;
                }
            }
        }

        public void NonMaskableInterrupt()
        {
            UnhaltIfHalted();
            PushProgramCounter();
            IFF2 = IFF1;
            IFF1 = false;
            PC = 0x0066; // There must be a handling routine here!

            // ToDo: wait 17 T-states
        }

        public Dictionary<ushort, string> Disassemble(ushort start, ushort end, List<(ushort From, ushort To)>? data = null)
        {
            var address = start;
            var retVal = new Dictionary<ushort, string>();
            var culture = new CultureInfo("en-US");

            while (address <= end)
            {
                if (data != null && data.Count > 0)
                {
                    if (data.Exists(d => d.From == address))
                    {
                        var (From, To) = data.Find(d => d.From == address);
                        address = (ushort)(To + 1);

                        for (ushort dataAddr = From; dataAddr < address; dataAddr++)
                        {
                            var dataByte = ReadFromBus(dataAddr);
                            var dataChar = (char)dataByte;
                            var display = $"{dataByte.ToString("X2", culture)}     {dataChar}";
                            retVal.Add(dataAddr, display);
                        }
                    }
                }

                var (addr, op, nextAddr) = DisassembleInstruction(address, culture);
                address = nextAddr;
                retVal.Add(addr, op);
            }

            return retVal;
        }

        public bool IsOpCodeSupported(string opCode)
        {
            var c = new CultureInfo("en-US");

            if (string.IsNullOrWhiteSpace(opCode) || !int.TryParse(opCode, NumberStyles.HexNumber, c, out var _))
            {
                return false;
            }

            opCode = opCode.ToUpper(c);

            return opCode.Length switch
            {
                2 => RootInstructions.ContainsKey(byte.Parse(opCode, NumberStyles.HexNumber, c)),
                4 => (opCode[0..2]) switch
                {
                    "DD" => DDInstructions.ContainsKey(byte.Parse(opCode[2..], NumberStyles.HexNumber, c)),
                    "FD" => FDInstructions.ContainsKey(byte.Parse(opCode[2..], NumberStyles.HexNumber, c)),
                    "ED" => EDInstructions.ContainsKey(byte.Parse(opCode[2..], NumberStyles.HexNumber, c)),
                    "CB" => CBInstructions.ContainsKey(byte.Parse(opCode[2..], NumberStyles.HexNumber, c)),
                    _ => false,
                },
                6 => (opCode[0..4]) switch
                {
                    "DDCB" => DDCBInstructions.ContainsKey(byte.Parse(opCode[4..], NumberStyles.HexNumber, c)),
                    "FDCB" => FDCBInstructions.ContainsKey(byte.Parse(opCode[4..], NumberStyles.HexNumber, c)),
                    _ => false,
                },
                8 => (opCode[0..4]) switch
                {
                    "DDCB" => DDCBInstructions.ContainsKey(byte.Parse(opCode[6..], NumberStyles.HexNumber, c)),
                    "FDCB" => FDCBInstructions.ContainsKey(byte.Parse(opCode[6..], NumberStyles.HexNumber, c)),
                    _ => false,
                },

                _ => false,
            };
        }

        private void Tick()
        {
            if (_clockCycles == 0)
            {
                var address = PC;
                _currentOpCode = ReadFromBus(address);
                var (opCode, operation, refresh) = FetchNextInstruction(_currentOpCode, ref address);
                PC = address;
                _currentOpCode = opCode;
                _clockCycles = operation.TStates;
                IncrementRefreshRegister(refresh);
                var additionalTStates = operation.Op(_currentOpCode);
                _clockCycles += additionalTStates;
                TotalTStates = TotalTStates + operation.TStates + additionalTStates;
            }

            _clockCycles--;
        }

        private byte ReadFromBus(ushort addr) => _bus.Read(addr, false);
        private void WriteToBus(ushort addr, byte data) => _bus.Write(addr, data);
        private byte ReadFromBusPort(ushort port) => _bus.ReadPeripheral(port);
        private void WriteToBusPort(ushort port, byte data) => _bus.WritePeripheral(port, data);

        private bool CheckFlag(Flags flag)
        {
            if ((F & flag) == flag)
            {
                return true;
            }

            return false;
        }

        private void SetFlag(Flags flag, bool value)
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

        private (byte opCode, Instruction operation, int refresh) PeekNextInstruction(byte code, ref ushort address) => 
            FetchNextInstruction(code, ref address);

        private (byte opCode, Instruction operation, int refresh) FetchNextInstruction(byte code, ref ushort address)
        {
            if (code != 0x76)
            {
                address++;
            }
            else
            {
                // Do not increment PC
                // Execute NOP
                IsHalted = true;
                return (0x00, new Instruction("NOP", IMP, IMP, NOP, new List<int> { 4 }), 1);
            }

            return GetInstruction(code, ref address);
        }

        private void ResetQ() => Q = (Flags)(0b00000000);

        private void SetQ() => Q = F;

        private void UnhaltIfHalted()
        {
            if (IsHalted)
            {
                IsHalted = false;
                PC++;
            }
        }

        private (ushort opAddress, string opString, ushort nextAddress) DisassembleInstruction(ushort address, CultureInfo c)
        {
            var opAddress = address;
            var aByte = ReadFromBus(address++);
            Instruction operation;

            (_, operation, _) = GetInstruction(aByte, ref address);

            var opCode = $"{operation.Mnemonic}";

            // Operands
            if (operation.AddressingMode1 == IMM)
            {
                var n = ReadFromBus(address++).ToString("X2", c);
                opCode = opCode.Replace("n", $"&{n}", StringComparison.InvariantCulture);
            }
            else if (operation.AddressingMode1 == REL)
            {
                var d = (sbyte)ReadFromBus(address++);
                var e = d > 0 ? d + 2 : d - 2;

                opCode = opCode.Replace("+d", $"{d.ToString("+0;-#", c)}", StringComparison.InvariantCulture);
                opCode = opCode.Replace("e", $"${e.ToString("+0;-#", c)}", StringComparison.InvariantCulture);
            }
            else if (operation.AddressingMode1 == RELS)
            {
                // Displacement sbyte is at address - 2, due to format of the instruction
                var d = (sbyte)ReadFromBus((ushort)(address-2));
                opCode = opCode.Replace("+d", $"{d.ToString("+0;-#", c)}", StringComparison.InvariantCulture);
            }
            else if (operation.AddressingMode1 == IMX)
            {
                var loByte = ReadFromBus(address++);
                var hiByte = (ushort)ReadFromBus(address++);
                var val = (ushort)((hiByte << 8) + loByte);
                var nn = val.ToString("X4", c);
                opCode = opCode.Replace("nn", $"&{nn}", StringComparison.InvariantCulture);
            }

            return (opAddress, opCode, address);
        }

        private (byte opCode, Instruction operation, int refresh) GetInstruction(byte code, ref ushort address)
        {
            // Note:
            // DDCB operations are in the form DDCB{displacement}{opCode}
            // FDCB operations are in the form FDCB{displacement}{opCode}
            switch (code)
            {
                case 0xCB:
                    var opCB = ReadFromBus(address);
                    address++;
                    return (opCB, CBInstructions[opCB], 2);
                case 0xDD:
                    var opDD = ReadFromBus(address);
                    address++;

                    if (opDD == 0xCB)
                    {
                        address++; // Skip over displacement operand d
                        var opDDCB = ReadFromBus(address);
                        address++;
                        return (opDDCB, DDCBInstructions[opDDCB], 2);
                    }

                    return (opDD, DDInstructions[opDD], 2);
                case 0xED:
                    var opED = ReadFromBus(address);
                    address++;
                    return (opED, EDInstructions[opED], 2);
                case 0xFD:
                    var opFD = ReadFromBus(address);
                    address++;

                    if (opFD == 0xCB)
                    {
                        address++; // Skip over displacement operand d
                        var opFDCB = ReadFromBus(address);
                        address++;
                        return (opFDCB, FDCBInstructions[opFDCB], 2);
                    }

                    return (opFD, FDInstructions[opFD], 2);
                default:
                    return (code, RootInstructions[code], 1);
            }
        }

        private void PushProgramCounter()
        {
            var loByte = (byte)(PC & 0xff);
            var hiByte = (byte)((PC >> 8) & 0xff);

            SP--;
            WriteToBus(SP, (byte)hiByte);
            SP--;
            WriteToBus(SP, loByte);
            ResetQ();
        }

        private void PopProgramCounter()
        {
            var loByte = ReadFromBus(SP);
            SP++;
            var hiByte = ReadFromBus(SP);
            SP++;

            PC = (ushort)((hiByte << 8) + loByte);
            ResetQ();
        }

        private void IncrementRefreshRegister(int steps)
        {
            var refresh = (byte)(R & 0b011111111);  // Only use bits 0-6
            var msbSet = (R & 0b10000000) > 0;
            refresh = (byte)(refresh + steps);
            R = refresh;

            if (msbSet)
            {
                R |= 0b10000000;
            }
            else
            {
                R &= 0b011111111;
            }
        }
    }
}
