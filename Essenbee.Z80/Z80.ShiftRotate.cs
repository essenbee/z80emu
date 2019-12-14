namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // Rotate and Shift Group
        // ========================================

        // Instruction    : RLCA
        // Operation      : A is rotated left 1 position, with bit 7 moving to the carry flag and bit 0
        // Flags Affected : H,N,C

        private byte RLCA(byte opCode)
        {
            var c = 0;

            if ((A & 0b10000000) > 0)
            {
                c = 1;
            }

            A = (byte)((A << 1) + c);

            // Do not use SetRotateLeftFlags() here
            SetFlag(Flags.C, c == 1);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction    : RLA
        // Operation      : A is rotated left 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 0
        // Flags Affected : H,N,C

        private byte RLA(byte opCode)
        {
            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = A & 0b10000000;

            A = (byte)((A << 1) + priorC);

            // Do not use SetRotateLeftFlags() here
            SetFlag(Flags.C, newC > 0);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction    : RRCA
        // Operation      : A is rotated right 1 position, with bit 0 moving to the carry flag and bit 7
        // Flags Affected : H,N,C

        private byte RRCA(byte opCode)
        {
            var c = 0;

            if ((A & 0b00000001) > 0)
            {
                c = 0b10000000;
            }

            A = (byte)((A >> 1) + c);

            // Do not use SetRotateRightFlags() here
            SetFlag(Flags.C, c == 0b10000000);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction    : RRA
        // Operation      : A is rotated right 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 7
        // Flags Affected : H,N,C

        private byte RRA(byte opCode)
        {
            var priorC = CheckFlag(Flags.C) ? 0b10000000 : 0;
            var newC = A & 0b00000001;

            A = (byte)((A >> 1) + priorC);

            // Do not use SetRotateRightFlags() here
            SetFlag(Flags.C, newC == 1);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction    : RLC r
        // Operation      : r is rotated left 1 position, with bit 7 moving to the carry flag and bit 0
        // Flags Affected : All

        private byte RLCR(byte opCode)
        {
            var c = 0;
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            if ((n & 0b10000000) > 0)
            {
                c = 1;
            }

            n = (byte)((n << 1) + c);

            AssignToRegister(src, n);

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RLC (HL)
        // Operation      : (HL) is rotated left 1 position, with bit 7 moving to the carry flag and bit 0
        // Flags Affected : All

        private byte RLCHL(byte opCode)
        {
            var c = 0;
            var n = Fetch1(CBInstructions);

            if ((n & 0b10000000) > 0)
            {
                c = 1;
            }

            n = (byte)((n << 1) + c);

            WriteToBus(HL, n);

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RLC (IX+d),r
        // Operation      : (IX+d) is rotated left 1 position, with bit 7 moving to the carry flag and bit 0.
        // Flags Affected : All

        private byte RLCIXD(byte opCode)
        {
            var c = 0;
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            var dest = opCode & 0b00000111;

            _absoluteAddress = (ushort)(IX + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            if ((n & 0b10000000) > 0)
            {
                c = 1;
            }

            n = (byte)((n << 1) + c);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RLC (IY+d)
        // Operation      : (IY+d) is rotated left 1 position, with bit 7 moving to the carry flag and bit 0
        // Flags Affected : All

        private byte RLCIYD(byte opCode)
        {
            var c = 0;

            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            if ((n & 0b10000000) > 0)
            {
                c = 1;
            }

            n = (byte)((n << 1) + c);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RL r
        // Operation      : r is rotated left 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 0
        // Flags Affected : All

        private byte RLR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b10000000;

            n = (byte)((n << 1) + priorC);

            SetFlag(Flags.C, newC > 0);

            AssignToRegister(src, n);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RL (HL)
        // Operation      : (HL) is rotated left 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 0
        // Flags Affected : All

        private byte RLHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b10000000;

            n = (byte)((n << 1) + priorC);

            SetFlag(Flags.C, newC > 0);

            WriteToBus(HL, n);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RL (IX+d),r
        // Operation      : (IX+d) is rotated left 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 0
        // Flags Affected : All

        private byte RLIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;

            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b10000000;

            n = (byte)((n << 1) + priorC);

            SetFlag(Flags.C, newC > 0);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RL (IY+d)
        // Operation      : (IY+d) is rotated left 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 0
        // Flags Affected : All

        private byte RLIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b10000000;

            n = (byte)((n << 1) + priorC);

            SetFlag(Flags.C, newC > 0);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RRC r
        // Operation      : r is rotated right 1 position, with bit 0 moving to the carry flag and bit 7
        // Flags Affected : All

        private byte RRCR(byte opCode)
        {
            var c = 0;
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            if ((n & 0b00000001) > 0)
            {
                c = 1;
            }

            n = (byte)(n >> 1);

            if (c == 1)
            {
                n |= 0b10000000;
            }

            AssignToRegister(src, n);

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RRC (HL)
        // Operation      : (HL) is rotated right 1 position, with bit 0 moving to the carry flag and bit 7
        // Flags Affected : All

        private byte RRCHL(byte opCode)
        {
            var c = 0;
            var n = Fetch1(CBInstructions);

            if ((n & 0b00000001) > 0)
            {
                c = 1;
            }

            n = (byte)(n >> 1);

            if (c == 1)
            {
                n |= 0b10000000;
            }

            WriteToBus(HL, n);

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RRC (IX+d),r
        // Operation      : (IX+d) is rotated right 1 position, with bit 0 moving to the carry flag and bit 70
        // Flags Affected : All

        private byte RRCIXD(byte opCode)
        {
            var c = 0;
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;

            _absoluteAddress = (ushort)(IX + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            if ((n & 0b00000001) > 0)
            {
                c = 1;
            }

            n = (byte)(n >> 1);

            if (c == 1)
            {
                n |= 0b10000000;
            }

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RRC (IY+d)
        // Operation      : (IY+d) is rotated right 1 position, with bit 0 moving to the carry flag and bit 70
        // Flags Affected : All

        private byte RRCIYD(byte opCode)
        {
            var c = 0;
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            if ((n & 0b00000001) > 0)
            {
                c = 1;
            }

            n = (byte)(n >> 1);

            if (c == 1)
            {
                n |= 0b10000000;
            }

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, c == 1);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RR r
        // Operation      : r is rotated right 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 7
        // Flags Affected : All

        private byte RRR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b00000001;

            n = (byte)(n >> 1);

            if (priorC == 1)
            {
                n |= 0b10000000;
            }

            SetFlag(Flags.C, newC > 0);

            AssignToRegister(src, n);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RR (HL)
        // Operation      : (HL) is rotated right 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 7
        // Flags Affected : All

        private byte RRHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b00000001;

            n = (byte)(n >> 1);

            if (priorC == 1)
            {
                n |= 0b10000000;
            }

            SetFlag(Flags.C, newC > 0);

            WriteToBus(HL, n);
            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RR (IX+d),r
        // Operation      : (IX+d) is rotated right 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 7
        // Flags Affected : All

        private byte RRIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;

            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b00000001;

            n = (byte)(n >> 1);

            if (priorC == 1)
            {
                n |= 0b10000000;
            }

            SetFlag(Flags.C, newC > 0);
            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : RR (IY+d)
        // Operation      : (IY+d) is rotated right 1 position, through the carry flag;
        //                : with the previous content of the carry flag copied to bit 7
        // Flags Affected : All

        private byte RRIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var priorC = CheckFlag(Flags.C) ? 1 : 0;
            var newC = n & 0b00000001;

            n = (byte)(n >> 1);

            if (priorC == 1)
            {
                n |= 0b10000000;
            }

            SetFlag(Flags.C, newC > 0);
            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetRotateFlags(n);

            return 0;
        }

        // Instruction    : SLA r
        // Operation      : r is shifted left 1 position, through the carry flag;
        //                : a 0 is shifted into bit 0
        // Flags Affected : All

        private byte SLAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var newCarry = n & 0b10000000;

            n = (byte)(n << 1);
            AssignToRegister(src, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLA (HL)
        // Operation      : (HL) is shifted left 1 position, through the carry flag;
        //                : a 0 is shifted into bit 0
        // Flags Affected : All

        private byte SLAHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);
            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);

            WriteToBus(HL, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLA (IX+d),r
        // Operation      : (IX+d) is shifted left 1 position, through the carry flag;
        //                : a 0 is shifted into bit 0
        // Flags Affected : All

        private byte SLAIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;

            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLA (IY+d)
        // Operation      : (IY+d) is shifted left 1 position, through the carry flag;
        //                : a 0 is shifted into bit 0
        // Flags Affected : All

        private byte SLAIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLS r
        // Operation      : r is shifted left 1 position, through the carry flag;
        //                : a 1 is shifted into bit 0
        // Flags Affected : All

        private byte SLSR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var newCarry = n & 0b10000000;

            n = (byte)(n << 1);
            n |= 0b00000001; //Set bit 0
            AssignToRegister(src, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLS (HL)
        // Operation      : (HL) is shifted left 1 position, through the carry flag;
        //                : a 1 is shifted into bit 0
        // Flags Affected : All

        private byte SLSHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);
            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);
            n |= 0b00000001; //Set bit 0
            WriteToBus(HL, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLS (IX+d),r
        // Operation      : (IX+d) is shifted left 1 position, through the carry flag;
        //                : a 1 is shifted into bit 0
        // Flags Affected : All

        private byte SLSIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;

            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);
            n |= 0b00000001; //Set bit 0

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SLS (IY+d)
        // Operation      : (IY+d) is shifted left 1 position, through the carry flag;
        //                : a 1 is shifted into bit 0
        // Flags Affected : All

        private byte SLSIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var newCarry = n & 0b10000000;
            n = (byte)(n << 1);
            n |= 0b00000001; //Set bit 0

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SRL r
        // Operation      : r is shifted right 1 position, through the carry flag;
        //                : a 0 is shifted into bit 7
        // Flags Affected : All

        private byte SRLR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var newCarry = n & 0b00000001;

            n = (byte)(n >> 1);
            AssignToRegister(src, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftRightLogicalFlags(n);

            return 0;
        }

        // Instruction    : SRL (HL)
        // Operation      : (HL) is shifted right 1 position, through the carry flag;
        //                : a 0 is shifted into bit 7
        // Flags Affected : All

        private byte SRLHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);

            var newCarry = n & 0b00000001;

            n = (byte)(n >> 1);
            WriteToBus(HL, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftRightLogicalFlags(n);

            return 0;
        }

        // Instruction    : SRL (IX+d)
        // Operation      : (IX+d) is shifted right 1 position, through the carry flag;
        //                : a 0 is shifted into bit 7
        // Flags Affected : All

        private byte SRLIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;
            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var newCarry = n & 0b00000001;

            n = (byte)(n >> 1);
            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftRightLogicalFlags(n);

            return 0;
        }

        // Instruction    : SRL (IY+d)
        // Operation      : (IY+d) is shifted right 1 position, through the carry flag;
        //                : a 0 is shifted into bit 7
        // Flags Affected : All

        private byte SRLIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var newCarry = n & 0b00000001;

            n = (byte)(n >> 1);
            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftRightLogicalFlags(n);

            return 0;
        }

        // Instruction    : SRA r
        // Operation      : r is shifted right 1 position; the contents of bit 0 are copied to the carry lag
        //                : and the previous contents of bit 7 remain unchanged.
        // Flags Affected : All

        private byte SRAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            var newCarry = n & 0b00000001;
            var bit7Set = (n & 0b10000000) > 0;

            n = (byte)(n >> 1);

            if (bit7Set)
            {
                n += 0b10000000;
            }

            AssignToRegister(src, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n);

            return 0;
        }

        // Instruction    : SRA (HL)
        // Operation      : (HL) is shifted right 1 position; the contents of bit 0 are copied to the carry lag
        //                : and the previous contents of bit 7 remain unchanged.
        // Flags Affected : All
        private byte SRAHL(byte opCode)
        {
            var n = Fetch1(CBInstructions);

            var newCarry = n & 0b00000001;
            var bit7Set = (n & 0b10000000) > 0;

            n = (byte)(n >> 1);

            if (bit7Set)
            {
                n += 0b10000000;
            }

            WriteToBus(HL, n);

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n); ;

            return 0;
        }

        // Instruction    : SRA (IX+d)
        // Operation      : (IX+d) is shifted right 1 position; the contents of bit 0 are copied to the carry lag
        //                : and the previous contents of bit 7 remain unchanged.
        // Flags Affected : All
        private byte SRAIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var dest = opCode & 0b00000111;
            MEMPTR = _absoluteAddress;
            var n = Fetch2(DDCBInstructions);

            var newCarry = n & 0b00000001;
            var bit7Set = (n & 0b10000000) > 0;

            n = (byte)(n >> 1);

            if (bit7Set)
            {
                n += 0b10000000;
            }

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n); ;

            return 0;
        }

        // Instruction    : SRA (IY+d)
        // Operation      : (IY+d) is shifted right 1 position; the contents of bit 0 are copied to the carry lag
        //                : and the previous contents of bit 7 remain unchanged.
        // Flags Affected : All
        private byte SRAIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            var dest = opCode & 0b00000111;
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = _absoluteAddress;
            var n = Fetch2(FDCBInstructions);

            var newCarry = n & 0b00000001;
            var bit7Set = (n & 0b10000000) > 0;

            n = (byte)(n >> 1);

            if (bit7Set)
            {
                n += 0b10000000;
            }

            WriteToBus(_absoluteAddress, n);

            if (dest != 6)
            {
                AssignToRegister(dest, n);
            }

            SetFlag(Flags.C, newCarry > 0);
            SetShiftArithmeticFlags(n); ;

            return 0;
        }

        // Instruction    : RLD
        // Operation      : Low nibble of (HL) <- Low nibble of A
        //                : Low nibble of A < - High nibble of (HL)
        //                : High nibble of (HL) <- Low nibble of (HL)
        // Flags Affected : All
        private byte RLD(byte opCode)
        {
            var n = Fetch1(EDInstructions);

            var lowNibbleA = (byte)(A & 0b00001111);
            var hiNibbleA = (byte)(A & 0b11110000);
            var lowNibbleN = (byte)(n & 0b00001111);
            var hiNibbleN = (byte)(n & 0b11110000);

            A = (byte)(hiNibbleA + (hiNibbleN >> 4));
            var newN = (byte)((lowNibbleN << 4) + lowNibbleA);

            WriteToBus(HL, newN);

            SetFlag(Flags.N, false);
            SetFlag(Flags.H, false);
            SetFlag(Flags.Z, A == 0);
            SetFlag(Flags.S, (sbyte)A < 0);
            SetFlag(Flags.P, Parity(A));
            // Carry flag is unaffected

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();
            MEMPTR = (ushort)(HL + 1);

            return 0;
        }

        // Instruction    : RRD
        // Operation      : High nibble of (HL) <- Low nibble of A
        //                : Low nibble of (HL) < - High nibble of (HL)
        //                : Low nibble of A <- Low nibble of (HL)
        // Flags Affected : All
        private byte RRD(byte opCode)
        {
            var n = Fetch1(EDInstructions);

            var lowNibbleA = (byte)(A & 0b00001111);
            var hiNibbleA = (byte)(A & 0b11110000);
            var lowNibbleN = (byte)(n & 0b00001111);
            var hiNibbleN = (byte)(n & 0b11110000);

            A = (byte)(hiNibbleA + lowNibbleN);
            var newN = (byte)((lowNibbleA << 4) + (hiNibbleN >> 4));

            WriteToBus(HL, newN);

            SetFlag(Flags.N, false);
            SetFlag(Flags.H, false);
            SetFlag(Flags.Z, A == 0);
            SetFlag(Flags.S, (sbyte)A < 0);
            SetFlag(Flags.P, Parity(A));
            // Carry flag is unaffected

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            MEMPTR = (ushort)(HL + 1);

            return 0;
        }
    }
}
