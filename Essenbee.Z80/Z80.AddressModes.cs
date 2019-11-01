using System;
using System.Collections.Generic;
using System.Text;

namespace Essenbee.Z80
{
    public partial class Z80
    {
        // Address Modes
        //
        // Z80 address modes refer to how the address of the data an instruction operates on,
        // is generated in each instruction. many instructions include more than one operand,
        // and in these cases, two types of addressing can be employed. For example, LD can use
        // Immediate Mode to specify the source data and Indexed Mode to specify the destination.

        // Implied Mode
        private byte IMP() => 0;

        // Indexed Mode
        private byte IDX() => 0;

        // Immediate Mode (called twice for Immediate Extended Mode)
        private byte IMM()
        {
            _absoluteAddress = PC++;
            return 0;
        }

        // Register Addressing:
        // The opcode contains bits of information that determine the registers involved
        private byte REG() => 0;

        // Register Indirect Addressing - (HL)
        private byte RGIHL()
        {
            _absoluteAddress = HL;

            return 0;
        }

        // Bit Addressing

        //
    }
}
