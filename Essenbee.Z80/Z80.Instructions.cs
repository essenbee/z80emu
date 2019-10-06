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
        // - DD or FD selects IX+d or IY+d respectively (in some cases without displacement d) in place of HL
        //
        // This means that we have to read another byte in order to detrmine the operation in this four cases.

        // No Operation
        private byte NOP()
        {
            return 0;
        }

        // ========================================
        // 8-bit Load Group
        // ========================================

        // Instruction: LD r, r'
        // Operation  : r <- r'


    }
}
