namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // Bit Set, Reset and Test Group
        // ========================================

        // Instruction    : BIT b,r
        // Operation      : Z <- true if bit is 0
        // Flags Affected : Z,H,N

        private byte BITBR(byte opCode)
        {
            var src = opCode & 0b00000111;
            byte n = ReadFromRegister(src);

            var bit = (opCode & 0b00111000) >> 3;
            var result = n & (byte)(1 << bit);

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, true);
            SetFlag(Flags.N, false);

            // Undocumented flags
            SetFlag(Flags.X, ((n & 0x0008) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((n & 0x0020) > 0) ? true : false); //Copy of bit 5
            SetFlag(Flags.S, bit == 7 && result != 0);
            SetFlag(Flags.P, result == 0);

            SetQ();

            return 0;
        }

        // Instruction    : BIT b,(HL)
        // Operation      : Z <- true if bit is 0
        // Flags Affected : Z,H,N

        private byte BITHL(byte opCode)
        {
            byte n = Fetch1(CBInstructions);

            var bit = (opCode & 0b00111000) >> 3;
            var result = n & (byte)(1 << bit);

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, true);
            SetFlag(Flags.N, false);

            // Undocumented flags
            SetFlag(Flags.X, ((MEMPTR & 0x0800) > 0) ? true : false); //Copy of bit 11
            SetFlag(Flags.U, ((MEMPTR & 0x2000) > 0) ? true : false); //Copy of bit 13
            SetFlag(Flags.S, bit == 7 && result != 0);
            SetFlag(Flags.P, result == 0);

            SetQ();

            return 0;
        }

        // Instruction    : BIT b,(IX+d)
        // Operation      : Z <- true if bit is 0
        // Flags Affected : Z,H,N

        private byte BITIXD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            _absoluteAddress = (ushort)(IX + d);
            MEMPTR = (ushort)(IX + d);
            var n = Fetch2(DDCBInstructions);

            var bit = (opCode & 0b00111000) >> 3;
            var result = n & (byte)(1 << bit);

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, true);
            SetFlag(Flags.N, false);

            // Undocumented flags
            SetFlag(Flags.X, ((MEMPTR & 0x0800) > 0) ? true : false); //Copy of bit 11
            SetFlag(Flags.U, ((MEMPTR & 0x2000) > 0) ? true : false); //Copy of bit 13
            SetFlag(Flags.S, bit == 7 && result != 0);
            SetFlag(Flags.P, result == 0);

            SetQ();

            return 0;
        }

        // Instruction    : BIT b,(IY+d)
        // Operation      : Z <- true if bit is 0
        // Flags Affected : Z,H,N

        private byte BITIYD(byte opCode)
        {
            var d = (sbyte)ReadFromBus((ushort)(PC - 2)); // displacement -128 to +127
            _absoluteAddress = (ushort)(IY + d);
            MEMPTR = (ushort)(IY + d);
            var n = Fetch2(FDCBInstructions);

            var bit = (opCode & 0b00111000) >> 3;
            var result = n & (byte)(1 << bit);

            SetFlag(Flags.Z, result == 0);
            SetFlag(Flags.H, true);
            SetFlag(Flags.N, false);

            // Undocumented flags
            SetFlag(Flags.X, ((MEMPTR & 0x0800) > 0) ? true : false); //Copy of bit 11
            SetFlag(Flags.U, ((MEMPTR & 0x2000) > 0) ? true : false); //Copy of bit 13
            SetFlag(Flags.S, bit == 7 && result != 0);
            SetFlag(Flags.P, result == 0);

            SetQ();

            return 0;
        }
    }
}
