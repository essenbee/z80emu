﻿using System;
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
        // Flags Affected: None
        private byte NOP(byte opCode) => 0;

        // Instruction    : HALT
        // Operation      : Execute NOPs until a subsequent interrupt or reset is received
        // Flags Affected: None
        private byte HALT(byte opCode)
        {
            // ToDo: Figure this out!
            return 0;
        }

        // ========================================
        // 8-bit Load Group
        // ========================================

        // Instruction   : LD r, r'
        // Operation     : r <- r'
        // Flags Affected: None
        private byte LDRR(byte opCode)
        {
            var src = opCode & 0b00000111;
            var dest = (opCode & 0b00111000) >> 3;

            var srcReg = 0;

            switch (src)
            {
                case 0:
                    srcReg = B;
                    break;
                case 1:
                    srcReg = C;
                    break;
                case 2:
                    srcReg = D;
                    break;
                case 3:
                    srcReg =E;
                    break;
                case 4:
                    srcReg = H;
                    break;
                case 5:
                    srcReg = L;
                    break;
                case 7:
                    srcReg = A;
                    break;
            }

            switch (dest)
            {
                case 0:
                    B = (byte)srcReg;
                    break;
                case 1:
                    C = (byte)srcReg;
                    break;
                case 2:
                    D = (byte)srcReg;
                    break;
                case 3:
                    E = (byte)srcReg;
                    break;
                case 4:
                    H = (byte)srcReg;
                    break;
                case 5:
                    L = (byte)srcReg;
                    break;
                case 7:
                    A = (byte)srcReg;
                    break;
            }

            return 0;
        }

        // Instruction   : LD r, n
        // Operation     : r <- n
        // Flags Affected: None
        private byte LDRN(byte opCode)
        {
            var dest = (opCode & 0b00111000) >> 3;
            var n = Fetch1();

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

            return 0;
        }
    }
}