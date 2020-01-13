namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // Input/Output Group
        // ========================================

        // Instruction    : IN A,(n)
        // Operation      : A <- (n)
        // Flags Affected : None
        private byte INA(byte opCode)
        {
            byte port = Fetch1(RootInstructions);

            var addr = (ushort)((A << 8) + port);
            var n = ReadFromBusPort(addr);

            MEMPTR = (ushort)(addr + 1);
            ResetQ();
            A = n;

            return 0;
        }

        // Instruction    : OUT (n),A
        // Operation      : (n) <- A
        // Flags Affected : None
        private byte OUTA(byte opCode)
        {
            byte port = Fetch1(RootInstructions);

            var addr = (ushort)((A << 8) + port);
            WriteToBusPort(addr, A);

            // MEMPTR_low = (port + 1) & #FF,  MEMPTR_hi = A
            MEMPTR = (ushort)((A << 8) + ((port + 1) & 0xFF));
            ResetQ();

            return 0;
        }

        // Instruction    : IN r,(C)
        // Operation      : r <- (C)
        // Flags Affected : None
        private byte INR(byte opCode)
        {
            var n = ReadFromBusPort(BC);
            MEMPTR = (ushort)(BC + 1);
            var dest = (opCode & 0b00111000) >> 3;
            AssignToRegister(dest, n);

            SetFlag(Flags.N, false);
            SetFlag(Flags.H, false);
            SetFlag(Flags.P, Parity(n));
            SetFlag(Flags.Z, n == 0);
            SetFlag(Flags.S, n > 0x7F);

            // Undocumented Flags
            SetFlag(Flags.X, ((n & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((n & 0x20) > 0) ? true : false); //Copy of bit 5

            SetQ();

            return 0;
        }

        // Instruction    : OUT (C),r
        // Operation      : (C) <- r
        // Flags Affected : None
        private byte OUTR(byte opCode)
        {
            MEMPTR = (ushort)(BC + 1);
            var src = (opCode & 0b00111000) >> 3;
            var n = ReadFromRegister(src);

            WriteToBusPort(BC, n);

            ResetQ();

            return 0;
        }

        // Instruction    : INI
        // Operation      : (HL) <- (C), B <- B - 1, HL <- HL + 1
        // Flags Affected : All
        private byte INI(byte opCode)
        {
            var n = ReadFromBusPort(BC);
            MEMPTR = (ushort)(BC + 1);
            WriteToBus(HL, n);

            IncRegisterPair(HL, 2);
            var val = B;
            B = (byte)(B - 1);
            var decVal = B;

            SetDecFlags(val, decVal);

            var k = n + ((C + 1) & 255);
            SetFlag(Flags.H, k > 0xFF);
            SetFlag(Flags.C, k > 0xFF);

            var x =(ushort)((k & 7) ^ decVal);
            SetFlag(Flags.P, Parity(x));
            SetFlag(Flags.N, ((n & 0x80) > 0) ? true : false); //Copy of bit 7

            SetQ();

            return 0;
        }
    }
}
