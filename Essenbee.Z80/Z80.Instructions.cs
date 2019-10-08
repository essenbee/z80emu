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
            var n = Fetch1();

            AssignToRegister(dest, n);

            return 0;
        }

        // Instruction   : LD r, (HL)
        // Operation     : r <- (HL) - that is, operand is located in the memory address pointed to by HL
        // Flags Affected: None
        private byte LDRHL(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var n = Fetch1();

            AssignToRegister(dest, n);

            return 0;
        }

        private byte ReadFromRegister(int src)
        {
            switch (src)
            {
                case 0:
                    return B;
                case 1:
                    return C;;
                case 2:
                    return D;
                case 3:
                    return E;
                case 4:
                    return H;
                case 5:
                    return L;
                case 7:
                    return A;
                default:
                    return 0x00;
            }
        }

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
