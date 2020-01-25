namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // 8-bit Arithmetic and Logic Group
        // ========================================

        // Instruction    : ADD A, n
        // Operation      : A <- A + n
        // Flags Affected : All except N
        private byte ADDAN(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            A = Add8(A, n);

            return 0;
        }

        // Instruction    : ADD A, r
        // Operation      : A <- A + r
        // Flags Affected : All except N
        private byte ADDAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);
            A = Add8(A, n);

            return 0;
        }

        // Instruction    : ADD A, (HL)
        // Operation      : A <- A + r
        // Flags Affected : All except N
        private byte ADDAHL(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            A = Add8(A, n);

            return 0;
        }

        // Instruction   : ADD A,(IX+d)
        // Operation     : A <- A + (IX+d)
        // Flags Affected: All except N
        private byte ADDAIXDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);
            A = Add8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : ADD A,(IY+d)
        // Operation     : A <- A + (IY+d)
        // Flags Affected: All except N
        private byte ADDAIYDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(DDInstructions);
            A = Add8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : ADC A, n
        // Operation      : A <- A + n + C
        // Flags Affected : All except N
        private byte ADCAN(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction    : ADC A, r
        // Operation      : A <- A + r + C
        // Flags Affected : All except N
        private byte ADCAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction    : ADC A, (HL)
        // Operation      : A <- A + (HL) + C
        // Flags Affected : All except N
        private byte ADCAHL(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction   : ADD A,(IX+d)
        // Operation     : A <- A + (IX+d)
        // Flags Affected: All except N
        private byte ADCAIXDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Add8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : ADD A,(IY+d)
        // Operation     : A <- A + (IY+d)
        // Flags Affected: All except N
        private byte ADCAIYDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(DDInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Add8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : SUB A, n
        // Operation      : A <- A - n
        // Flags Affected : All
        private byte SUBAN(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction    : SUB A, r
        // Operation      : A <- A - r
        // Flags Affected : All
        private byte SUBAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction    : SUB A, (HL)
        // Operation      : A <- A - (HL)
        // Flags Affected : All
        private byte SUBAHL(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction   : SUB A,(IX+d)
        // Operation     : A <- A - (IX+d)
        // Flags Affected: All
        private byte SUBAIXDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);
            A = Sub8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : SUB A,(IY+d)
        // Operation     : A <- A - (IY+d)
        // Flags Affected: All
        private byte SUBAIYDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(DDInstructions);
            A = Sub8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : SBC A, n
        // Operation      : A <- A - n - C
        // Flags Affected : All
        private byte SBCAN(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction    : SBC A, r
        // Operation      : A <- A - r - C
        // Flags Affected : All
        private byte SBCAR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction    : SBC A, (HL)
        // Operation      : A <- A - (HL) - C
        // Flags Affected : All
        private byte SBCAHL(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction   : SBC A,(IX+d)
        // Operation     : A <- A - (IX+d) - C
        // Flags Affected: All
        private byte SBCAIXDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Sub8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : SBC A,(IY+d)
        // Operation     : A <- A - (IY+d) - C
        // Flags Affected: All
        private byte SBCAIYDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(DDInstructions);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);
            A = Sub8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : INC r
        // Operation      : r <- r + 1
        // Flags Affected : S,Z,H,P/V,N
        private byte INCR(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var src = dest;

            byte val = ReadFromRegister(src);
            var incVal = (byte)(val + 1);
            AssignToRegister(dest, incVal);

            SetIncFlags(val, incVal);

            return 0;
        }

        // Instruction    : INC (HL)
        // Operation      : (HL) <- (HL) + 1
        // Flags Affected : S,Z,H,P/V,N
        private byte INCHL(byte opCode)
        {
            var val = Fetch1(RootInstructions);

            var incVal = (byte)(val + 1);
            WriteToBus(HL, incVal);

            SetIncFlags(val, incVal);

            return 0;
        }

        // Instruction   : INC (IX+d)
        // Operation     : (IX+d) <- (IX+d) + 1
        // Flags Affected: S,Z,H,P/V,N
        private byte INCIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var val = Fetch2(DDInstructions);

            var incVal = (byte)(val + 1);
            WriteToBus(_absoluteAddress, incVal);

            SetIncFlags(val, incVal);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : INC (IY+d)
        // Operation     : (IY+d) <- (IY+d) + 1
        // Flags Affected: S,Z,H,P/V,N
        private byte INCIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var val = Fetch2(FDInstructions);

            var incVal = (byte)(val + 1);
            WriteToBus(_absoluteAddress, incVal);

            SetIncFlags(val, incVal);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : DEC r
        // Operation      : r <- r - 1
        // Flags Affected : S,Z,H,P/V,N
        private byte DECR(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var src = dest;

            byte val = ReadFromRegister(src);
            var decVal = (byte)(val - 1);
            AssignToRegister(dest, decVal);

            SetDecFlags(val, decVal);

            return 0;
        }

        // Instruction    : DEC (HL)
        // Operation      : (HL) <- (HL) - 1
        // Flags Affected : S,Z,H,P/V,N
        private byte DECHL(byte opCode)
        {
            var val = Fetch1(RootInstructions);

            var incVal = (byte)(val - 1);
            WriteToBus(HL, incVal);

            SetDecFlags(val, incVal);

            return 0;
        }

        // Instruction   : DEC (IX+d)
        // Operation     : (IX+d) <- (IX+d) - 1
        // Flags Affected: S,Z,H,P/V,N
        private byte DECIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var val = Fetch2(DDInstructions);

            var incVal = (byte)(val - 1);
            WriteToBus(_absoluteAddress, incVal);

            SetDecFlags(val, incVal);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : DEC (IY+d)
        // Operation     : (IY+d) <- (IY+d) - 1
        // Flags Affected: S,Z,H,P/V,N
        private byte DECIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var val = Fetch2(FDInstructions);

            var incVal = (byte)(val - 1);
            WriteToBus(_absoluteAddress, incVal);

            SetDecFlags(val, incVal);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : CP r
        // Operation      : Compare r with A
        // Flags Affected : S,Z,H,P/V,N,C
        private byte CPR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);
            var diff = A - n;

            SetComparisonFlags(n, diff);

            return 0;
        }

        // Instruction    : CP n
        // Operation      : Compare n with A
        // Flags Affected : S,Z,H,P/V,N,C
        private byte CPN(byte opCode)
        {
            byte n = Fetch1(RootInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff);

            return 0;
        }

        // Instruction    : CP (HL)
        // Operation      : Compare (HL) with A
        // Flags Affected : S,Z,H,P/V,N,C
        private byte CPHL(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff); ;

            return 0;
        }

        // Instruction   : CP (IX+d)
        // Operation     : Compare (IX+d) with A
        // Flags Affected: S,Z,H,P/V,N,C
        private byte CPIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : CP (IY+d)
        // Operation     : Compare (IY+d) with A
        // Flags Affected: S,Z,H,P/V,N,C
        private byte CPIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(FDInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : AND r
        // Operation     : A <- A & r
        // Flags Affected: All
        private byte ANDR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);
            A = And(A, n);

            return 0;
        }

        // Instruction   : AND n
        // Operation     : A <- A & n
        // Flags Affected: All
        private byte ANDN(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = And(A, n);

            return 0;
        }

        // Instruction   : AND (HL)
        // Operation     : A <- A & (HL)
        // Flags Affected: All
        private byte ANDHL(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = And(A, n);

            return 0;
        }

        // Instruction   : AND (IX+d)
        // Operation     : A <- A & (IX+d)
        // Flags Affected: All
        private byte ANDIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);

            A = And(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : AND (IY+d)
        // Operation     : A <- A & (IY+d)
        // Flags Affected: All
        private byte ANDIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(FDInstructions);

            A = And(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : OR r
        // Operation     : A <- A | r
        // Flags Affected: All
        private byte ORR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);
            A = Or(A, n);

            return 0;
        }

        // Instruction   : OR n
        // Operation     : A <- A | n
        // Flags Affected: All
        private byte ORN(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = Or(A, n);

            return 0;
        }

        // Instruction   : OR (HL)
        // Operation     : A <- A | (HL)
        // Flags Affected: All
        private byte ORHL(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = Or(A, n);

            return 0;
        }

        // Instruction   : OR (IX+d)
        // Operation     : A <- A | (IX+d)
        // Flags Affected: All
        private byte ORIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);

            A = Or(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : OR (IY+d)
        // Operation     : A <- A | (IY+d)
        // Flags Affected: All
        private byte ORIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(FDInstructions);

            A = Or(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : XOR r
        // Operation     : A <- A ^ r
        // Flags Affected: All
        private byte XORR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);
            A = Xor(A, n);

            return 0;
        }

        // Instruction   : XOR n
        // Operation     : A <- A ^ n
        // Flags Affected: All
        private byte XORN(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = Xor(A, n);

            return 0;
        }

        // Instruction   : XOR (HL)
        // Operation     : A <- A ^ (HL)
        // Flags Affected: All
        private byte XORHL(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            A = Xor(A, n);

            return 0;
        }

        // Instruction   : XOR (IX+d)
        // Operation     : A <- A ^ (IX+d)
        // Flags Affected: All
        private byte XORIXD(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);

            A = Xor(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : XOR (IY+d)
        // Operation     : A <- A ^ (IY+d)
        // Flags Affected: All
        private byte XORIYD(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(FDInstructions);

            A = Xor(A, n);
            MEMPTR = _absoluteAddress;

            return 0;
        }

        // ========================================
        // 16-bit Arithmetic Group
        // ========================================

        // Instruction    : ADD HL, ss
        // Operation      : HL <- HL + ss
        // Flags Affected : H,C,N

        private byte ADDHLSS(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, HL);

            var sum = Add16(HL, n);

            H = (byte)((sum & 0xFF00) >> 8);
            L = (byte)(sum & 0x00FF);

            return 0;
        }

        // Instruction    : ADC HL, ss
        // Operation      : HL <- HL + ss + C
        // Flags Affected : H,C,N

        private byte ADCHLSS(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, HL);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);

            var sum = Add16WithCarry(HL, n, c);

            H = (byte)((sum & 0xFF00) >> 8);
            L = (byte)(sum & 0x00FF);

            return 0;
        }

        // Instruction    : SBC HL, ss
        // Operation      : HL <- HL - ss - C
        // Flags Affected : All

        private byte SBCHLSS(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, HL);
            byte c = CheckFlag(Flags.C) ? ((byte)0x01) : ((byte)0x00);

            var diff = Sub16(HL, n, c);

            H = (byte)((diff & 0xFF00) >> 8);
            L = (byte)(diff & 0x00FF);

            return 0;
        }

        // Instruction    : ADD IX, pp
        // Operation      : IX <- IX + pp
        // Flags Affected : H,N,C

        private byte ADDIXPP(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, IX);

            MEMPTR = (ushort)(IX + 1);

            IX = Add16(IX, n);

            return 0;
        }

        // Instruction    : ADD IY, pp
        // Operation      : IY <- IY + pp
        // Flags Affected : H,N,C

        private byte ADDIYPP(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, IY);

            MEMPTR = (ushort)(IY + 1);

            IY = Add16(IY, n);

            return 0;
        }

        // Instruction    : INC ss
        // Operation      : ss <- ss + 1
        // Flags Affected : None

        private byte INCSS(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, HL);

            n++;

            WriteToRegisterPair(src, n);

            return 0;
        }

        // Instruction    : INC IX
        // Operation      : IX <- IX + 1
        // Flags Affected : None

        private byte INCIX(byte opCode)
        {
            IX++;
            return 0;
        }

        // Instruction    : INC IXh
        // Operation      : IXl <- IXh + 1
        // Flags Affected : 

        private byte INCIXH(byte opCode)
        {
            var hiByte = (byte)(IX >> 8);
            var loByte = IX & 0x00FF;
            var incVal = (byte)(hiByte + 1);
            IX = (ushort)((incVal << 8) + loByte);

            SetIncFlags(hiByte, incVal);

            return 0;
        }

        // Instruction    : INC IXl
        // Operation      : IXl <- IXl + 1
        // Flags Affected : 

        private byte INCIXL(byte opCode)
        {
            var hiByte = (byte)(IX >> 8);
            var loByte = (byte)(IX & 0x00FF);
            var incVal = (byte)(loByte + 1);

            IX = (ushort)((hiByte << 8) + incVal);

            SetIncFlags(loByte, incVal);

            return 0;
        }

        // Instruction    : INC IY
        // Operation      : IY <- IY + 1
        // Flags Affected : None

        private byte INCIY(byte opCode)
        {
            IY++;
            return 0;
        }

        // Instruction    : INC IYh
        // Operation      : IYh <- IYh + 1
        // Flags Affected : 

        private byte INCIYH(byte opCode)
        {
            var hiByte = (byte)(IY >> 8);
            var loByte = IY & 0x00FF;
            var incVal = (byte)(hiByte + 1);
            IY = (ushort)((incVal << 8) + loByte);

            SetIncFlags(hiByte, incVal);

            return 0;
        }

        // Instruction    : INC IYl
        // Operation      : IYl <- IYl + 1
        // Flags Affected : 

        private byte INCIYL(byte opCode)
        {
            var hiByte = (byte)(IY >> 8);
            var loByte = (byte)(IY & 0x00FF);
            var incVal = (byte)(loByte + 1);

            IY = (ushort)((hiByte << 8) + incVal);

            SetIncFlags(loByte, incVal);

            return 0;
        }

        // Instruction    : INC ss
        // Operation      : ss <- ss - 1
        // Flags Affected : None

        private byte DECSS(byte opCode)
        {
            var src = (opCode & 0b00110000) >> 4;
            var n = ReadFromRegisterPair(src, HL);

            n--;

            WriteToRegisterPair(src, n);

            return 0;
        }

        // Instruction    : DEC IX
        // Operation      : IX <- IX - 1
        // Flags Affected : None

        private byte DECIX(byte opCode)
        {
            IX--;
            return 0;
        }

        // Instruction    : DEC IY
        // Operation      : IY <- IY - 1
        // Flags Affected : None

        private byte DECIY(byte opCode)
        {
            IY--;
            return 0;
        }

        // Instruction    : DEC IXh
        // Operation      : IXh <- IXh - 1
        // Flags Affected : 

        private byte DECIXH(byte opCode)
        {
            var hiByte = (byte)(IX >> 8);
            var loByte = IX & 0x00FF;
            var incVal = (byte)(hiByte - 1);
            IX = (ushort)((incVal << 8) + loByte);

            SetDecFlags(hiByte, incVal);

            return 0;
        }

        // Instruction    : DEC IYh
        // Operation      : IYh <- IYh - 1
        // Flags Affected : 

        private byte DECIYH(byte opCode)
        {
            var hiByte = (byte)(IY >> 8);
            var loByte = IY & 0x00FF;
            var incVal = (byte)(hiByte - 1);
            IY = (ushort)((incVal << 8) + loByte);

            SetDecFlags(hiByte, incVal);

            return 0; ;
        }

        // Instruction    : DEC IXl
        // Operation      : IXl <- IXl - 1
        // Flags Affected : 

        private byte DECIXL(byte opCode)
        {
            var hiByte = (byte)(IX >> 8);
            var loByte = (byte)(IX & 0x00FF);
            var incVal = (byte)(loByte - 1);

            IX = (ushort)((hiByte << 8) + incVal);

            SetDecFlags(loByte, incVal);

            return 0;
        }

        // Instruction    : DEC IYl
        // Operation      : IYl <- IYl - 1
        // Flags Affected : 

        private byte DECIYL(byte opCode)
        {
            var hiByte = (byte)(IY >> 8);
            var loByte = (byte)(IY & 0x00FF);
            var incVal = (byte)(loByte - 1);

            IY = (ushort)((hiByte << 8) + incVal);

            SetDecFlags(loByte, incVal);

            return 0;
        }
    }
}
