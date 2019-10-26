using System;
using System.Collections.Generic;
using System.Text;

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
        // This means that we have to read another byte in order to detrmine the operation in this four cases.

        // Instruction    : NOP
        // Operation      : No Operation
        // Flags Affected : None
        private byte NOP(byte opCode) => 0;

        // Instruction    : HALT
        // Operation      : Execute NOPs until a subsequent interrupt or reset is received
        // Flags Affected : None
        // Notes          : The HALT instruction halts the Z80; it does not increase the PC so that the
        //                  instruction is reexecuted, until a maskable or non-maskable interrupt is accepted.
        //                  Only then does the Z80 increase the PC again and continues with the next instruction.
        //                  During the HALT state, the HALT line is set. The PC is increased before the interrupt
        //                  routine is called.
        private byte HALT(byte opCode)
        {
            // ToDo: Figure this out!
            return 0;
        }

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

            return 0;
        }

        // Instruction   : LD (HL),n
        // Operation     : (HL) <- n - that is, n is loaded into the memory address pointed to by HL
        // Flags Affected: None
        private byte LDHLN(byte opCode)
        {
            var n = Fetch1(_rootInstructions);
            WriteToBus(HL, n);

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

            return 0;
        }

        // Instruction   : LD A,(BC)
        // Operation     : A <- (BC)
        // Flags Affected: None
        private byte LDABC(byte opCode)
        {
            _absoluteAddress = (ushort)(BC);
            var n = Fetch1(_rootInstructions);
            A = n;

            return 0;
        }

        // Instruction   : LD A,(DE)
        // Operation     : A <- (DE)
        // Flags Affected: None
        private byte LDADE(byte opCode)
        {
            _absoluteAddress = (ushort)(DE);
            var n = Fetch1(_rootInstructions);
            A = n;

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

            return 0;
        }

        // Instruction   : LD (BC),A
        // Operation     : (BC) <- A - that is, r is loaded into the memory address pointed to by BC
        // Flags Affected: None
        private byte LDBCA(byte opCode)
        {
            WriteToBus(BC, A);

            return 0;
        }


        // Instruction   : LD (DE),A
        // Operation     : (DE) <- A - that is, r is loaded into the memory address pointed to by DE
        // Flags Affected: None
        private byte LDDEA(byte opCode)
        {
            WriteToBus(DE, A);

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
            A = Add8(A, n,c );

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

            return 0;
        }

        private byte Add8(byte a, byte b, byte c = 0)
        {
            var sum = a + b + c;

            SetFlag(Flags.N, false);
            SetFlag(Flags.Z, sum == 0 ? true : false);
            SetFlag(Flags.S, (sum & 0x80) > 0 ? true : false);
            SetFlag(Flags.H, (a & 0x0F) + (b & 0x0F) > 0xF ? true : false);
            SetFlag(Flags.P, (a >= 0x80 && b >= 0x80) ||
                (a < 0x80 && b < 0x80 && sum < 0) 
                ? true : false);
            SetFlag(Flags.C, sum > 0xFF ? true : false); // Set if there is a carry into bit 8

            // Undocumented Flags
            SetFlag(Flags.X, (sum & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (sum & 0x20) > 0 ? true : false); //Copy of bit 5

            return (byte)sum;
        }

        private byte Sub8(byte a, byte b, byte c = 0)
        {
            var diff = a - b - c;

            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, diff == 0 ? true : false);
            SetFlag(Flags.S, (diff & 0x80) > 0 ? true : false);
            SetFlag(Flags.H, ((a & 0x0F) < (b & 0x0F) + c) ? true : false);
            SetFlag(Flags.P, (a >= 0x80 && b >= 0x80 && (sbyte)diff > 0 || 
                (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
                ? true : false);
            SetFlag(Flags.C, diff > 0xFF ? true : false); // Set if there is not a borrow from bit 8

            // Undocumented Flags
            SetFlag(Flags.X, (diff & 0x08) > 0 ? true : false); //Copy of bit 3
            SetFlag(Flags.U, (diff & 0x20) > 0 ? true : false); //Copy of bit 5

            return (byte)diff;
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
    }
}
