namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // 8-bit Load Group
        // ========================================

        // Instruction    : LD r, r'
        // Operation      : r <- r'
        // Flags Affected : None
        private byte LDRR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var dest = (opCode & 0b00111000) >> 3;

            byte srcReg = ReadFromRegister(src);
            AssignToRegister(dest, srcReg);
            ResetQ();

            return 0;
        }

        // Instruction   : LD r, n
        // Operation     : r <- n
        // Flags Affected: None
        private byte LDRN(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var n = Fetch1(RootInstructions);

            AssignToRegister(dest, n);
            ResetQ();

            return 0;
        }

        // Instruction   : LD r, (HL)
        // Operation     : r <- (HL) - that is, operand is located in the memory address pointed to by HL
        // Flags Affected: None
        private byte LDRHL(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var n = Fetch1(RootInstructions);

            AssignToRegister(dest, n);
            ResetQ();

            return 0;
        }

        // Instruction   : LD r, (IX+d)
        // Operation     : r <- (IX+d)
        // Flags Affected: None
        private byte LDRIXD(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(DDInstructions);

            AssignToRegister(dest, n);

            MEMPTR = _absoluteAddress;
            ResetQ();

            return 0;
        }

        // Instruction   : LD r, (IY+d)
        // Operation     : r <- (IY+d)
        // Flags Affected: None
        private byte LDRIYD(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127

            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(FDInstructions);

            AssignToRegister(dest, n);

            MEMPTR = _absoluteAddress;
            ResetQ();

            return 0;
        }

        // Instruction   : LD (HL),r
        // Operation     : (HL) <- r - that is, r is loaded into the memory address pointed to by HL
        // Flags Affected: None
        private byte LDHLR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);

            WriteToBus(HL, n);
            ResetQ();

            return 0;
        }

        // Instruction   : LD (IX+d),r
        // Operation     : (IX+d) <- r
        // Flags Affected: None
        private byte LDIXDR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            var addr = (ushort)(IX + d);

            WriteToBus(addr, n);

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction   : LD (IY+d),r
        // Operation     : (IY+d) <- r
        // Flags Affected: None
        private byte LDIYDR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var n = ReadFromRegister(src);
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            var addr = (ushort)(IY + d);

            WriteToBus(addr, n);

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction   : LD (HL),n
        // Operation     : (HL) <- n - that is, n is loaded into the memory address pointed to by HL
        // Flags Affected: None
        private byte LDHLN(byte opCode)
        {
            var n = Fetch1(RootInstructions);
            WriteToBus(HL, n);

            ResetQ();

            return 0;
        }

        // Instruction   : LD (IX+d),n
        // Operation     : (IX+d) <- n
        // Flags Affected: None
        private byte LDIXDN(byte opCode)
        {
            var d = (sbyte)Fetch1(DDInstructions); // displacement -128 to +127
            var n = Fetch2(DDInstructions);
            var addr = (ushort)(IX + d);

            WriteToBus(addr, n);

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction   : LD (IY+d),n
        // Operation     : (IY+d) <- n
        // Flags Affected: None
        private byte LDIYDN(byte opCode)
        {
            var d = (sbyte)Fetch1(FDInstructions); // displacement -128 to +127
            var n = Fetch2(FDInstructions);
            var addr = (ushort)(IY + d);

            WriteToBus(addr, n);

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction   : LD A,(BC)
        // Operation     : A <- (BC)
        // Flags Affected: None
        private byte LDABC(byte opCode)
        {
            _absoluteAddress = BC;
            var n = Fetch1(RootInstructions);
            A = n;

            ResetQ();
            MEMPTR = (ushort)(BC + 1);

            return 0;
        }

        // Instruction   : LD A,(DE)
        // Operation     : A <- (DE)
        // Flags Affected: None
        private byte LDADE(byte opCode)
        {
            _absoluteAddress = DE;
            var n = Fetch1(RootInstructions);
            A = n;

            ResetQ();
            MEMPTR = (ushort)(DE + 1);

            return 0;
        }

        // Instruction   : LD A,(nn)
        // Operation     : A <- (nn)
        // Flags Affected: None
        private byte LDANN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);

            var addr = (hiByte << 8) + loByte;
            _absoluteAddress = (ushort)addr;
            var n = Fetch2(RootInstructions);
            A = n;

            ResetQ();
            MEMPTR = (ushort)(addr + 1);

            return 0;
        }

        // Instruction   : LD (BC),A
        // Operation     : (BC) <- A - that is, A is loaded into the memory address pointed to by BC
        // Flags Affected: None
        private byte LDBCA(byte opCode)
        {
            WriteToBus(BC, A);

            ResetQ();
            MEMPTR = (ushort)((A << 8) + ((BC + 1) & 0xFF));

            return 0;
        }


        // Instruction   : LD (DE),A
        // Operation     : (DE) <- A - that is, A is loaded into the memory address pointed to by DE
        // Flags Affected: None
        private byte LDDEA(byte opCode)
        {
            WriteToBus(DE, A);

            ResetQ();
            MEMPTR = (ushort)((A << 8) + ((DE + 1) & 0xFF));

            return 0;
        }

        // Instruction   : LD (nn),A
        // Operation     : (nn) <- A
        // Flags Affected: None
        private byte LDNNA(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);

            var addr = (ushort)((hiByte << 8) + loByte);
            WriteToBus(addr, A);

            MEMPTR = (ushort)((A << 8) + ((addr + 1) & 0xFF));
            ResetQ();

            return 0;
        }

        // Instruction    : LD A,I
        // Operation      : A <- I (interrupt vector)
        // Flags Affected : S,Z,H,P/V,N
        private byte LDAI(byte opCode)
        {
            A = I;

            var signedI = (sbyte)I;

            SetFlag(Flags.S, signedI < 0);
            SetFlag(Flags.Z, I == 0);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5

            // ToDo: if an interrupt occurs during this instruction, reset P/V
            SetFlag(Flags.P, IFF2);
            SetQ();

            return 0;
        }

        // Instruction    : LD A,R
        // Operation      : A <- R (refresh)
        // Flags Affected : S,Z,H,P/V,N
        private byte LDAR(byte opCode)
        {
            A = R;

            var signedR = (sbyte)R;

            SetFlag(Flags.S, signedR < 0);
            SetFlag(Flags.Z, R == 0);
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((A & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((A & 0x20) > 0) ? true : false); //Copy of bit 5

            // ToDo: if an interrupt occurs during this instruction, reset P/V
            SetFlag(Flags.P, IFF2);
            SetQ();

            return 0;
        }

        // Instruction    : LD I,A
        // Operation      : I <- A
        // Flags Affected : None
        private byte LDIA(byte opCode)
        {
            I = A;
            ResetQ();

            return 0;
        }

        // Instruction    : LD R,A
        // Operation      : R <- A
        // Flags Affected : None
        private byte LDRA(byte opCode)
        {
            R = A;
            ResetQ();

            return 0;
        }

        // ========================================
        // 16-bit Load Group
        // ========================================

        // Instruction   : LD BC,nn
        // Operation     : BC <- nn
        // Flags Affected: None
        private byte LDBCNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);

            B = hiByte;
            C = loByte;

            ResetQ();

            return 0;
        }

        // Instruction   : LD DE,nn
        // Operation     : DE <- nn
        // Flags Affected: None
        private byte LDDENN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);

            D = hiByte;
            E = loByte;

            ResetQ();

            return 0;
        }

        // Instruction   : LD HL,nn
        // Operation     : HL <- nn
        // Flags Affected: None
        private byte LDHLNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);

            H = hiByte;
            L = loByte;
            ResetQ();

            return 0;
        }

        // Instruction   : LD SP,nn
        // Operation     : SP <- nn
        // Flags Affected: None
        private byte LDSPNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);

            SP = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD IX,nn
        // Operation     : IX <- nn
        // Flags Affected: None
        private byte LDIXNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);

            IX = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD IY,nn
        // Operation     : IY <- nn
        // Flags Affected: None
        private byte LDIYNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);

            IY = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD HL,(nn)
        // Operation     : H <- (nn+1), L <- (nn)
        // Flags Affected: None
        private byte LDHLFNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            L = Fetch2(RootInstructions);
            _absoluteAddress = (ushort)hiAddr;
            H = Fetch2(RootInstructions);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD BC,(nn)
        // Operation     : B <- (nn+1), C <- (nn)
        // Flags Affected: None
        private byte LDBCFNN(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            C = Fetch2(EDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            B = Fetch2(EDInstructions);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD DE,(nn)
        // Operation     : B <- (nn+1), C <- (nn)
        // Flags Affected: None
        private byte LDDEFNN(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            E = Fetch2(EDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            D = Fetch2(EDInstructions);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD HL,(nn)
        // Operation     : H <- (nn+1), L <- (nn)
        // Flags Affected: None
        private byte LDHLFNN2(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            L = Fetch2(EDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            H = Fetch2(EDInstructions);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD SP,(nn)
        // Operation     : SP <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDSPFNN(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(EDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(EDInstructions);

            var operand = (ushort)((hi << 8) + lo);
            SP = operand;

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD IX,(nn)
        // Operation     : IX <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDIXFNN(byte opCode)
        {
            var loByte = Fetch1(DDInstructions);
            var hiByte = (ushort)Fetch1(DDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(DDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(DDInstructions);

            var operand = (ushort)((hi << 8) + lo);

            IX = operand;

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD IY,(nn)
        // Operation     : IY <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDIYFNN(byte opCode)
        {
            var loByte = Fetch1(FDInstructions);
            var hiByte = (ushort)Fetch1(FDInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(FDInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(FDInstructions);

            var operand = (ushort)((hi << 8) + lo);

            IY = operand;
            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), HL
        // Operation     : (nn+1) <- H, (nn) <- L
        // Flags Affected: None
        private byte LDNNHL(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = (ushort)Fetch1(RootInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, L);
            WriteToBus(hiAddr, H);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), BC
        // Operation     : (nn+1) <- C, (nn) <- C
        // Flags Affected: None
        private byte LDNNBC(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, C);
            WriteToBus(hiAddr, B);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), DE
        // Operation     : (nn+1) <- D, (nn) <- E
        // Flags Affected: None
        private byte LDNNDE(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, E);
            WriteToBus(hiAddr, D);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), HL
        // Operation     : (nn+1) <- H, (nn) <- L
        // Flags Affected: None
        private byte LDNNHL2(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, L);
            WriteToBus(hiAddr, H);

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), SP
        // Operation     : (nn+1) <- S, (nn) <- P
        // Flags Affected: None
        private byte LDNNSP(byte opCode)
        {
            var loByte = Fetch1(EDInstructions);
            var hiByte = (ushort)Fetch1(EDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(SP & 0xff)); // P
            WriteToBus(hiAddr, (byte)((SP >> 8) & 0xff)); // S

            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), IX
        // Operation     : (nn+1) <- I, (nn) <- X
        // Flags Affected: None
        private byte LDNNIX(byte opCode)
        {
            var loByte = Fetch1(DDInstructions);
            var hiByte = (ushort)Fetch1(DDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(IX & 0xff)); // X
            WriteToBus(hiAddr, (byte)((IX >> 8) & 0xff)); // I
            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD (nn), IY
        // Operation     : (nn+1) <- I, (nn) <- Y
        // Flags Affected: None
        private byte LDNNIY(byte opCode)
        {
            var loByte = Fetch1(FDInstructions);
            var hiByte = (ushort)Fetch1(FDInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(IY & 0xff)); // Y
            WriteToBus(hiAddr, (byte)((IY >> 8) & 0xff)); // I
            ResetQ();
            MEMPTR = (ushort)((hiByte << 8) + loByte + 1);

            return 0;
        }

        // Instruction   : LD SP, HL
        // Operation     : SP <- HL
        // Flags Affected: None
        private byte LDSPHL(byte opCode)
        {
            SP = HL;
            ResetQ();

            return 0;
        }

        // Instruction   : LD SP, IX
        // Operation     : SP <- IX
        // Flags Affected: None
        private byte LDSPIX(byte opCode)
        {
            SP = IX;
            ResetQ();

            return 0;
        }

        // Instruction   : LD SP, IY
        // Operation     : SP <- IY
        // Flags Affected: None
        private byte LDSPIY(byte opCode)
        {
            SP = IY;
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH BC
        // Operation     : (SP-2) <- C, (SP-1) <- B
        // Flags Affected: None
        private byte PUSHBC(byte opCode)
        {
            SP--;
            WriteToBus(SP, B);
            SP--;
            WriteToBus(SP, C);
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH DE
        // Operation     : (SP-2) <- E, (SP-1) <- D
        // Flags Affected: None
        private byte PUSHDE(byte opCode)
        {
            SP--;
            WriteToBus(SP, D);
            SP--;
            WriteToBus(SP, E);
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH HL
        // Operation     : (SP-2) <- L, (SP-1) <- H
        // Flags Affected: None
        private byte PUSHHL(byte opCode)
        {
            SP--;
            WriteToBus(SP, H);
            SP--;
            WriteToBus(SP, L);
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH AF
        // Operation     : (SP-2) <- F, (SP-1) <- A
        // Flags Affected: None
        private byte PUSHAF(byte opCode)
        {
            SP--;
            WriteToBus(SP, A);
            SP--;
            WriteToBus(SP, (byte)F);
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH IX
        // Operation     : (SP-2) <- X, (SP-1) <- I
        // Flags Affected: None
        private byte PUSHIX(byte opCode)
        {
            var x = (byte)(IX & 0xff);
            var i = (byte)((IX >> 8) & 0xff);

            SP--;
            WriteToBus(SP, i);
            SP--;
            WriteToBus(SP, x);
            ResetQ();

            return 0;
        }

        // Instruction   : PUSH IY
        // Operation     : (SP-2) <- Y, (SP-1) <- I
        // Flags Affected: None
        private byte PUSHIY(byte opCode)
        {
            var y = (byte)(IY & 0xff);
            var i = (byte)((IY >> 8) & 0xff);

            SP--;
            WriteToBus(SP, i);
            SP--;
            WriteToBus(SP, y);
            ResetQ();

            return 0;
        }

        // Instruction   : POP BC
        // Operation     : B <- (SP+1), C <- (SP)
        // Flags Affected: None
        private byte POPBC(byte opCode)
        {
            C = ReadFromBus(SP);
            SP++;
            B = ReadFromBus(SP);
            SP++;
            ResetQ();

            return 0;
        }

        // Instruction   : POP DE
        // Operation     : D <- (SP+1), E <- (SP)
        // Flags Affected: None
        private byte POPDE(byte opCode)
        {
            E = ReadFromBus(SP);
            SP++;
            D = ReadFromBus(SP);
            SP++;
            ResetQ();

            return 0;
        }

        // Instruction   : POP HL
        // Operation     : H <- (SP+1), L <- (SP)
        // Flags Affected: None
        private byte POPHL(byte opCode)
        {
            L = ReadFromBus(SP);
            SP++;
            H = ReadFromBus(SP);
            SP++;
            ResetQ();

            return 0;
        }

        // Instruction   : POP AF
        // Operation     : A <- (SP+1), F <- (SP)
        // Flags Affected: None
        private byte POPAF(byte opCode)
        {
            F = (Flags)ReadFromBus(SP);
            SP++;
            A = ReadFromBus(SP);
            SP++;
            ResetQ();

            return 0;
        }

        // Instruction   : POP IX
        // Operation     : I <- (SP+1), X <- (SP)
        // Flags Affected: None
        private byte POPIX(byte opCode)
        {
            var x = ReadFromBus(SP);
            SP++;
            var i = ReadFromBus(SP);
            SP++;

            IX = (ushort)((i << 8) + x);
            ResetQ();

            return 0;
        }

        // Instruction   : POP IY
        // Operation     : I <- (SP+1), Y <- (SP)
        // Flags Affected: None
        private byte POPIY(byte opCode)
        {
            var y = ReadFromBus(SP);
            SP++;
            var i = ReadFromBus(SP);
            SP++;

            IY = (ushort)((i << 8) + y);
            ResetQ();

            return 0;
        }

    }
}
