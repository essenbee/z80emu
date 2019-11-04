using System;

namespace Essenbee.Z80
{
    public partial class Z80
    {
        // Z80 Instructions
        //
        // Note: The Z80 uses 252 out of the available 256 codes as single byte opcodes ("root instructions").
        // The four remaining codes are used extensively as opcode "prefixes":
        // 
        // - CB and ED enable extra instructions
        // - DD or FD selects IX+d or IY+d respectively (in some cases without the displacement d) in place of HL
        //
        // This means that we have to read another byte in order to determine the operation in these four cases.

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
            var n = Fetch1(_rootInstructions);

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
            var n = Fetch1(_rootInstructions);

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
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127

            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);

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
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127

            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_fdInstructions);

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
            var src = (opCode & 0b00000111);
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
            var src = (opCode & 0b00000111);
            var n = ReadFromRegister(src);
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
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
            var src = (opCode & 0b00000111);
            var n = ReadFromRegister(src);
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127
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
            var n = Fetch1(_rootInstructions);
            WriteToBus(HL, n);

            ResetQ();

            return 0;
        }

        // Instruction   : LD (IX+d),n
        // Operation     : (IX+d) <- n
        // Flags Affected: None
        private byte LDIXDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            var n = Fetch2(_ddInstructions);
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
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127
            var n = Fetch2(_fdInstructions);
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
            var n = Fetch1(_rootInstructions);
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
            var n = Fetch1(_rootInstructions);
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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);

            var addr = (hiByte << 8) + loByte;
            _absoluteAddress = (ushort)addr;
            var n = Fetch2(_rootInstructions);
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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);

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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = Fetch1(_rootInstructions);

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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = Fetch1(_rootInstructions);

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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = Fetch1(_rootInstructions);

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
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);

            SP = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD IX,nn
        // Operation     : IX <- nn
        // Flags Affected: None
        private byte LDIXNN(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);

            IX = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD IY,nn
        // Operation     : IY <- nn
        // Flags Affected: None
        private byte LDIYNN(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);

            IY = (ushort)((hiByte << 8) + loByte);
            ResetQ();

            return 0;
        }

        // Instruction   : LD HL,(nn)
        // Operation     : H <- (nn+1), L <- (nn)
        // Flags Affected: None
        private byte LDHLFNN(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            L = Fetch2(_rootInstructions);
            _absoluteAddress = (ushort)hiAddr;
            H = Fetch2(_rootInstructions);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD BC,(nn)
        // Operation     : B <- (nn+1), C <- (nn)
        // Flags Affected: None
        private byte LDBCFNN(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            C = Fetch2(_edInstructions);
            _absoluteAddress = (ushort)hiAddr;
            B = Fetch2(_edInstructions);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD DE,(nn)
        // Operation     : B <- (nn+1), C <- (nn)
        // Flags Affected: None
        private byte LDDEFNN(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            E = Fetch2(_edInstructions);
            _absoluteAddress = (ushort)hiAddr;
            D = Fetch2(_edInstructions);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD HL,(nn)
        // Operation     : H <- (nn+1), L <- (nn)
        // Flags Affected: None
        private byte LDHLFNN2(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            L = Fetch2(_edInstructions);
            _absoluteAddress = (ushort)hiAddr;
            H = Fetch2(_edInstructions);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD SP,(nn)
        // Operation     : SP <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDSPFNN(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(_edInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(_edInstructions);

            var operand = (ushort)((hi << 8) + lo);
            SP = operand;

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD IX,(nn)
        // Operation     : IX <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDIXFNN(byte opCode)
        {
            var loByte = Fetch1(_ddInstructions);
            var hiByte = (ushort)Fetch1(_ddInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(_ddInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(_ddInstructions);

            var operand = (ushort)((hi << 8) + lo);

            IX = operand;

            ResetQ();

            return 0;
        }

        // Instruction   : LD IY,(nn)
        // Operation     : IY <- (nn+1), <- (nn)
        // Flags Affected: None
        private byte LDIYFNN(byte opCode)
        {
            var loByte = Fetch1(_fdInstructions);
            var hiByte = (ushort)Fetch1(_fdInstructions);
            var loAddr = (hiByte << 8) + loByte;
            var hiAddr = loAddr + 1;

            _absoluteAddress = (ushort)loAddr;
            var lo = Fetch2(_fdInstructions);
            _absoluteAddress = (ushort)hiAddr;
            var hi = Fetch2(_fdInstructions);

            var operand = (ushort)((hi << 8) + lo);

            IY = operand;
            ResetQ();

            return 0;
        }

        // Instruction   : LD (nn), HL
        // Operation     : (nn+1) <- H, (nn) <- L
        // Flags Affected: None
        private byte LDNNHL(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = (ushort)Fetch1(_rootInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, L);
            WriteToBus(hiAddr, H);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD (nn), BC
        // Operation     : (nn+1) <- C, (nn) <- C
        // Flags Affected: None
        private byte LDNNBC(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, C);
            WriteToBus(hiAddr, B);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD (nn), DE
        // Operation     : (nn+1) <- D, (nn) <- E
        // Flags Affected: None
        private byte LDNNDE(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, E);
            WriteToBus(hiAddr, D);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD (nn), HL
        // Operation     : (nn+1) <- H, (nn) <- L
        // Flags Affected: None
        private byte LDNNHL2(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, L);
            WriteToBus(hiAddr, H);

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD (nn), SP
        // Operation     : (nn+1) <- S, (nn) <- P
        // Flags Affected: None
        private byte LDNNSP(byte opCode)
        {
            var loByte = Fetch1(_edInstructions);
            var hiByte = (ushort)Fetch1(_edInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(SP & 0xff)); // P
            WriteToBus(hiAddr, (byte)((SP >> 8) & 0xff)); // S

            ResetQ();
            // ToDo: MEMPTR

            return 0;
        }

        // Instruction   : LD (nn), IX
        // Operation     : (nn+1) <- I, (nn) <- X
        // Flags Affected: None
        private byte LDNNIX(byte opCode)
        {
            var loByte = Fetch1(_ddInstructions);
            var hiByte = (ushort)Fetch1(_ddInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(IX & 0xff)); // X
            WriteToBus(hiAddr, (byte)((IX >> 8) & 0xff)); // I
            ResetQ();

            return 0;
        }

        // Instruction   : LD (nn), IY
        // Operation     : (nn+1) <- I, (nn) <- Y
        // Flags Affected: None
        private byte LDNNIY(byte opCode)
        {
            var loByte = Fetch1(_fdInstructions);
            var hiByte = (ushort)Fetch1(_fdInstructions);
            var loAddr = (ushort)((hiByte << 8) + loByte);
            var hiAddr = (ushort)(loAddr + 1);

            WriteToBus(loAddr, (byte)(IY & 0xff)); // Y
            WriteToBus(hiAddr, (byte)((IY >> 8) & 0xff)); // I
            ResetQ();

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

        // ========================================
        // 8-bit Arithmetic and Logic Group
        // ========================================

        // Instruction    : ADD A, n
        // Operation      : A <- A + n
        // Flags Affected : All except N
        private byte ADDAN(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            A = Add8(A, n);

            return 0;
        }

        // Instruction    : ADD A, r
        // Operation      : A <- A + r
        // Flags Affected : All except N
        private byte ADDAR(byte opCode)
        {
            var src = (opCode & 0b00000111);
            byte n = ReadFromRegister(src);
            A = Add8(A, n);

            return 0;
        }

        // Instruction    : ADD A, (HL)
        // Operation      : A <- A + r
        // Flags Affected : All except N
        private byte ADDAHL(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            A = Add8(A, n);

            return 0;
        }

        // Instruction   : ADD A,(IX+d)
        // Operation     : A <- A + (IX+d)
        // Flags Affected: All except N
        private byte ADDAIXDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);
            A = Add8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : ADD A,(IY+d)
        // Operation     : A <- A + (IY+d)
        // Flags Affected: All except N
        private byte ADDAIYDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_ddInstructions);
            A = Add8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : ADC A, n
        // Operation      : A <- A + n + C
        // Flags Affected : All except N
        private byte ADCAN(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction    : ADC A, r
        // Operation      : A <- A + r + C
        // Flags Affected : All except N
        private byte ADCAR(byte opCode)
        {
            var src = (opCode & 0b00000111);
            byte n = ReadFromRegister(src);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction    : ADC A, (HL)
        // Operation      : A <- A + (HL) + C
        // Flags Affected : All except N
        private byte ADCAHL(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Add8(A, n, c);

            return 0;
        }

        // Instruction   : ADD A,(IX+d)
        // Operation     : A <- A + (IX+d)
        // Flags Affected: All except N
        private byte ADCAIXDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Add8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : ADD A,(IY+d)
        // Operation     : A <- A + (IY+d)
        // Flags Affected: All except N
        private byte ADCAIYDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_ddInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Add8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : SUB A, n
        // Operation      : A <- A - n
        // Flags Affected : All
        private byte SUBAN(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction    : SUB A, r
        // Operation      : A <- A - r
        // Flags Affected : All
        private byte SUBAR(byte opCode)
        {
            var src = (opCode & 0b00000111);
            byte n = ReadFromRegister(src);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction    : SUB A, (HL)
        // Operation      : A <- A - (HL)
        // Flags Affected : All
        private byte SUBAHL(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            A = Sub8(A, n);

            return 0;
        }

        // Instruction   : SUB A,(IX+d)
        // Operation     : A <- A - (IX+d)
        // Flags Affected: All
        private byte SUBAIXDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);
            A = Sub8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : SUB A,(IY+d)
        // Operation     : A <- A - (IY+d)
        // Flags Affected: All
        private byte SUBAIYDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_ddInstructions);
            A = Sub8(A, n);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction    : SBC A, n
        // Operation      : A <- A - n - C
        // Flags Affected : All
        private byte SBCAN(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction    : SBC A, r
        // Operation      : A <- A - r - C
        // Flags Affected : All
        private byte SBCAR(byte opCode)
        {
            var src = (opCode & 0b00000111);
            byte n = ReadFromRegister(src);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction    : SBC A, (HL)
        // Operation      : A <- A - (HL) - C
        // Flags Affected : All
        private byte SBCAHL(byte opCode)
        {
            byte n = Fetch1(_rootInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Sub8(A, n, c);

            return 0;
        }

        // Instruction   : SBC A,(IX+d)
        // Operation     : A <- A - (IX+d) - C
        // Flags Affected: All
        private byte SBCAIXDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
            A = Sub8(A, n, c);

            MEMPTR = _absoluteAddress;

            return 0;
        }

        // Instruction   : SBC A,(IY+d)
        // Operation     : A <- A - (IY+d) - C
        // Flags Affected: All
        private byte SBCAIYDN(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_ddInstructions);
            byte c = CheckFlag(Flags.C) ? (byte)0x01 : (byte)0x00;
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
            var val = Fetch1(_rootInstructions);

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
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var val = Fetch2(_ddInstructions);

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
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var val = Fetch2(_fdInstructions);

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
            var val = Fetch1(_rootInstructions);

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
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var val = Fetch2(_ddInstructions);

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
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var val = Fetch2(_fdInstructions);

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
            byte n = Fetch1(_rootInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff);

            return 0;
        }

        // Instruction    : CP (HL)
        // Operation      : Compare (HL) with A
        // Flags Affected : S,Z,H,P/V,N,C
        private byte CPHL(byte opCode)
        {
            var n = Fetch1(_rootInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff); ;

            return 0;
        }

        // Instruction   : CP (IX+d)
        // Operation     : Compare (IX+d) with A
        // Flags Affected: S,Z,H,P/V,N,C
        private byte CPIXD(byte opCode)
        {
            sbyte d = (sbyte)Fetch1(_ddInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            var n = Fetch2(_ddInstructions);
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
            sbyte d = (sbyte)Fetch1(_fdInstructions); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            var n = Fetch2(_fdInstructions);
            var diff = A - n;

            SetComparisonFlags(n, diff);
            MEMPTR = _absoluteAddress;

            return 0;
        }





        // ========================================
        // General Purpose Arithmetic/CPU Control
        // ========================================

        // Instruction   : DAA
        // Operation     : Conditionally adjusts A for BCD arithmetic.
        // Flags Affected: S,Z,H,P,C
        private byte DAA(byte opCode)
        {
            var t = 0;

            if (CheckFlag(Flags.H) || ((A & 0xF) > 9))
            {
                t++;
            }

            // Set the Carry flag here ...
            if (CheckFlag(Flags.C) || (A > 0x99))
            {
                t += 2;
                SetFlag(Flags.C, true);
            }

            // Determine the Half-carry Flag here...
            if (CheckFlag(Flags.N) && !CheckFlag(Flags.H))
            {
                SetFlag(Flags.H, false);
            }
            else
            {
                if (CheckFlag(Flags.N) && CheckFlag(Flags.H))
                {
                    SetFlag(Flags.H, (A & 0x0F) < 6);
                }
                else
                {
                    SetFlag(Flags.H, (A & 0x0F) >= 0x0A);
                }
            }

            // Add or subtract 6 to/from nibbles as required, to adjust A for BCD correctness...
            switch (t)
            {
                case 1:
                    A += CheckFlag(Flags.N) ? (byte)0xFA : (byte)0x06; // -6:6
                    break;
                case 2:
                    A += CheckFlag(Flags.N) ? (byte)0xA0 : (byte)0x60; // -0x60:0x60
                    break;
                case 3:
                    A += CheckFlag(Flags.N) ? (byte)0x9A : (byte)0x66; // -0x66:0x66
                    break;
            }

            // Other Flags
            SetFlag(Flags.S, (A & 0x80) > 0 ? true : false);
            SetFlag(Flags.Z, A == 0);
            SetFlag(Flags.P, Parity(A));

            // Undocumented Flags
            SetFlag(Flags.X, (A & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (A & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction   : CPL
        // Operation     : A <- Ones Complement of A
        // Flags Affected: H,N
        private byte CPL(byte opCode)
        {
            A = (byte)~A;

            SetFlag(Flags.H, true);
            SetFlag(Flags.N, true);
            SetQ();

            return 0;
        }

        // Instruction   : NEG
        // Operation     : A <- Twos Complement of A (negation)
        // Flags Affected: All
        private byte NEG(byte opCode)
        {
            var temp = A;
            A = (byte)~A;
            A++;

            SetFlag(Flags.N, true);
            SetFlag(Flags.S, (A & 0x80) > 0 ? true : false);
            SetFlag(Flags.C, temp != 0 ? true : false);
            SetFlag(Flags.P, temp == 0x80 ? true : false);
            SetFlag(Flags.Z, A == 0 ? true : false);
            SetFlag(Flags.H, (temp & 0x0F) + ((~temp + 1) & 0x0F) > 0xF ? true : false);

            // Undocumented Flags
            SetFlag(Flags.X, (A & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (A & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();

            return 0;
        }

        // Instruction   : CCF
        // Operation     : Invert Carry Flag
        // Flags Affected: H,N,C
        private byte CCF(byte opCode)
        {
            var temp = CheckFlag(Flags.C);
            SetFlag(Flags.C, !temp);
            SetFlag(Flags.N, false);;
            SetFlag(Flags.H, temp);

            // Undocumented Flags set as per Patrik Rak
            //` https://www.worldofspectrum.org/forums/discussion/41704/redirect/p1
            var x = (byte)(((byte)Q ^ (byte)F) | A);
            SetFlag(Flags.X, (x & 0x08) > 0 ? true : false);
            SetFlag(Flags.U, (x & 0x20) > 0 ? true : false);

            SetQ();

            return 0;
        }

        // Instruction   : SCF
        // Operation     : Set Carry Flag
        // Flags Affected: H,N,C
        private byte SCF(byte opCode)
        {
            SetFlag(Flags.C, true);
            SetFlag(Flags.N, false); ;
            SetFlag(Flags.H, false);

            // Undocumented Flags set as per Patrik Rak
            //` https://www.worldofspectrum.org/forums/discussion/41704/redirect/p1
            var x = (byte)(((byte)Q ^ (byte)F) | A);
            SetFlag(Flags.X, (x & 0x08) > 0 ? true : false);
            SetFlag(Flags.U, (x & 0x20) > 0 ? true : false);

            SetQ();

            return 0;
        }

        // Instruction    : NOP
        // Operation      : No Operation
        // Flags Affected : None
        private byte NOP(byte opCode) => 0;

        // Instruction    : HALT
        // Operation      : Execute NOPs until a subsequent interrupt or reset is received
        // Flags Affected : None
        // Notes          : The HALT instruction halts the Z80; it does not increase the PC so that the
        //                  instruction is re-executed, until a maskable or non-maskable interrupt is accepted.
        //                  Only then does the Z80 increase the PC again and continues with the next instruction.
        //                  During the HALT state, the HALT line is set. The PC is increased before the interrupt
        //                  routine is called.
        private byte HALT(byte opCode)
        {
            // ToDo: Figure this out!

            ResetQ();
            return 0;
        }

        // Instruction    : DI
        // Operation      : Disable maskable interrupts
        // Flags Affected : None
        private byte DI(byte opCode)
        {
            IFF1 = false;
            IFF2 = false;
            ResetQ();

            return opCode;
        }

        // Instruction    : EI
        // Operation      : Enable maskable interrupts
        // Flags Affected : None
        // Notes          : Interrupts are not accepted immediately after an EI, but are accepted
        //                  after the next instruction.
        private byte EI(byte opCode)
        {
            // ToDo: only allow interrupts after the next instruction is executed
            IFF1 = true;
            IFF2 = true;
            ResetQ();

            return opCode;
        }


        // Instruction    : IM0
        // Operation      : Set Interrupt Mode 0
        // Flags Affected : None
        // Notes          : In the maskable interrupt mode 0, an interrupting device places
        //                  an instruction on the data bus for execution by the Z80. The
        //                  instruction is normally a Restart (RST) instruction since this is
        //                  an efficient one byte call to any one of eight subroutines located
        //                  in the first 64 bytes of memory (each subroutine is 8 bytes long).
        //                  However, any instruction may be given to the Z80­. The first byte of
        //                  a multi-byte instruction is read during the interrupt acknowledge cycle.
        //                  Subsequent bytes are read in by a normal memory read sequence (the PC,
        //                  however, remains at its pre­-interrupt state and the user must ensure
        //                  that memory will not respond to these read sequences). When the
        //                  interrupt is recognized, further interrupts are automatically disabled
        //                  (IFF1 and IFF2 are false). Any time after the interrupt sequence begins,
        //                  EI can be executed, meaning that this subroutine itself can be interrupted. 
        //                  This process may continue to any level as  long as all pertinent data are 
        //                  saved and restored. A CPU reset will automatically set interrupt mode 0.

        private byte IM0(byte opCode)
        {
            InterruptMode = InterruptMode.Mode0;
            ResetQ();

            return opCode;
        }

        // Instruction    : IM1
        // Operation      : Set Interrupt Mode 0
        // Flags Affected : None
        // Notes          : This maskable mode allows peripherals of minimal complexity interrupt 
        //                  access. In this respect, it is similar to the NMI interrupt except that
        //                  the CPU does an automatic CALL to location 0038H instead of 0066H. As in 
        //                  the NMI, the CPU automatically pushes the PC onto the Stack. Note that 
        //                  when doing programmed I/O, the CPU will ignore any data put onto the data 
        //                  bus during the interrupt acknowledge cycle.

        private byte IM1(byte opCode)
        {
            InterruptMode = InterruptMode.Mode1;
            ResetQ();

            return opCode;
        }

        // Instruction    : IM2
        // Operation      : Set Interrupt Mode 0
        // Flags Affected : None
        // Notes          : The Z80­ supports an interrupt vectoring structure that allows a peripheral 
        //                  device to identify the starting location of an interrupt service routine. 
        //                  Mode 2 is the most powerful of the three maskable interrupt modes allowing an 
        //                  indirect call to any memory location by a single 8-bit vector supplied from 
        //                  a peripheral. In this mode a peripheral generating the interrupt places 
        //                  the vector on the data bus in response to an interrupt acknowledge. This vector 
        //                  then becomes the least significant 8-bits of the indirect pointer while the I 
        //                  register in the CPU provides the most significant 8 bits. This address in turn
        //                  points to an address in a vector table which is the starting address of the interrupt 
        //                  routine. Interrupt processing thus starts at an arbitrary 16-bit address allowing any 
        //                  location in memory to be the start of the service routine. Notice that since the 
        //                  vector is used to identify two adjacent bytes to form a 16-bit address, only 7 
        //                  bits are required for the vector and the least significant bit is is zero.

        private byte IM2(byte opCode)
        {
            InterruptMode = InterruptMode.Mode2;
            ResetQ();

            return opCode;
        }

        // ========================================
        // 16-bit Arithmetic Group
        // ========================================



        // ========================================
        // Jump Group
        // ========================================

        // Instruction    : JP nn
        // Operation      : PC <- nn
        // Flags Affected : None
        private byte JPNN(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = Fetch1(_rootInstructions);
            var addr = (ushort)((hiByte << 8) + loByte);

            PC = addr;

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction    : JP cc, nn
        // Operation      : PC <- nn if cc is true
        // Flags Affected : None
        private byte JPCCNN(byte opCode)
        {
            var loByte = Fetch1(_rootInstructions);
            var hiByte = Fetch1(_rootInstructions);
            var addr = (ushort)((hiByte << 8) + loByte);

            var cc = (opCode & 0b00111000) >> 3;

            if (EvaluateCC(cc))
            {
                PC = addr;
                MEMPTR = addr;
            }

            ResetQ();
            return 0;
        }

        // Instruction    : JR e
        // Operation      : PC <- PC + e
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JR(byte opCode)
        {
            var e = (sbyte)Fetch1(_rootInstructions);

            PC = (ushort)(PC + e);

            ResetQ();
            MEMPTR = PC;

            return 0;
        }

        // Instruction    : JR C, e
        // Operation      : PC <- PC + e if Carry set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRC(byte opCode)
        {
            var e = (sbyte)Fetch1(_rootInstructions);

            if (CheckFlag(Flags.C))
            {
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return 0;
        }

        // Instruction    : JR NC, e
        // Operation      : PC <- PC + e if Carry not set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRNC(byte opCode)
        {
            var e = (sbyte)Fetch1(_rootInstructions);

            if (!CheckFlag(Flags.C))
            {
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return 0;
        }







        // =========================== H E L P E R S ===========================

        private byte Add8(byte a, byte b, byte c = 0)
        {
            var sum = (a + b + c);

            SetFlag(Flags.N, false);
            SetFlag(Flags.Z, (byte)sum == 0 ? true : false);
            SetFlag(Flags.S, ((byte)sum & 0x80) > 0 ? true : false);
            SetFlag(Flags.H, (a & 0x0F) + (b & 0x0F) > 0xF ? true : false);

            // Overflow flag
            if (((a ^ (b + c)) & 0x80) == 0 // Same sign
                && ((a ^ sum) & 0x80) != 0) // Different sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, sum > 0xFF ? true : false); // Set if there is a carry into bit 8

            // Undocumented Flags
            SetFlag(Flags.X, ((byte)sum & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((byte)sum & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();

            return (byte)sum;
        }

        private byte Sub8(byte a, byte b, byte c = 0)
        {
            var diff = a - b - c;

            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, (byte)diff == 0 ? true : false);
            SetFlag(Flags.S, ((byte)diff & 0x80) > 0 ? true : false);
            SetFlag(Flags.H, ((a & 0x0F) < ((b + c) & 0x0F)) ? true : false);

            // Overflow flag
            if (((a ^ (b + c)) & 0x80) != 0              // Different sign
                && (((b + c) ^ (byte)diff) & 0x80) == 0) // Same sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, diff < 0 ? true : false); // Set if there is not a borrow from bit 8

            // Undocumented Flags
            SetFlag(Flags.X, ((byte)diff & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((byte)diff & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();

            return (byte)diff;
        }

        private void SetIncFlags(byte val, byte incVal)
        {
            SetFlag(Flags.N, false);
            SetFlag(Flags.S, (sbyte)incVal < 0);
            SetFlag(Flags.P, val == 0x7F);
            SetFlag(Flags.Z, incVal == 0);
            SetFlag(Flags.H, (val & 0x0F) + (0x01 & 0x0F) > 0xF ? true : false);

            // Undocumented Flags
            SetFlag(Flags.X, (incVal & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (incVal & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();
        }

        private void SetDecFlags(byte val, byte decVal)
        {
            SetFlag(Flags.N, true);
            SetFlag(Flags.S, (sbyte)decVal < 0);
            SetFlag(Flags.P, val == 0x80);
            SetFlag(Flags.Z, decVal == 0);
            SetFlag(Flags.H, ((val & 0x0F) < (0x01 & 0x0F)) ? true : false);

            // Undocumented Flags
            SetFlag(Flags.X, (decVal & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (decVal & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();
        }

        private void SetComparisonFlags(byte n, int diff)
        {
            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, diff == 0);
            SetFlag(Flags.S, diff < 0);

            SetFlag(Flags.H, ((A & 0x0F) < (n & 0x0F)) ? true : false);

            // Overflow flag
            if (((A ^ n) & 0x80) != 0              // Different sign
                && ((n ^ (byte)diff) & 0x80) == 0) // Same sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, diff < 0 ? true : false); // Set if there is not a borrow from bit 8

            // Undocumented Flags
            SetFlag(Flags.X, ((byte)diff & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((byte)diff & 0x20) > 0 ? true : false); //Copy of bit 5
            SetQ();
        }

        private static bool Parity(ushort res)
        {
            var retVal = true;

            while (res > 0)
            {
                if ((res & 0x01) == 1) retVal = !retVal;
                res = (byte)(res >> 1);
            }

            return retVal;
        }

        private byte ReadFromRegister(int src) => 
            src switch
            {
                0 => B,
                1 => C,
                2 => D,
                3 => E,
                4 => H,
                5 => L,
                7 => A,
                _ => 0x00
            };

        private void AssignToRegister(int dest, byte n)
        {
            switch (dest)
            {
                case 0:
                    B = (byte)n;
                    break;
                case 1:
                    C = (byte)n;
                    break;
                case 2:
                    D = (byte)n;
                    break;
                case 3:
                    E = (byte)n;
                    break;
                case 4:
                    H = (byte)n;
                    break;
                case 5:
                    L = (byte)n;
                    break;
                case 7:
                    A = (byte)n;
                    break;
            }
        }

        private bool EvaluateCC(int cc) =>
            cc switch
            {
                0 => !CheckFlag(Flags.Z),
                1 => CheckFlag(Flags.Z),
                2 => !CheckFlag(Flags.C),
                3 => CheckFlag(Flags.C),
                4 => !Parity(A),
                5 => Parity(A),
                6 => !CheckFlag(Flags.S),
                7 => CheckFlag(Flags.S),
                _ => false
            };
    }
}
