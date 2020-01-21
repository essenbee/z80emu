namespace Essenbee.Z80
{
    public partial class Z80
    {
        // =========================== H E L P E R S ===========================

        private byte Add8(byte a, byte b, byte c = 0)
        {
            var sum = a + b + c;

            SetFlag(Flags.N, false);
            SetFlag(Flags.Z, ((byte)sum == 0) ? true : false);
            SetFlag(Flags.S, ((((byte)sum) & 0x80) > 0) ? true : false);
            SetFlag(Flags.H, ((a & 0x0F) + (b & 0x0F) > 0xF) ? true : false);

            // Overflow flag
            if ((((a ^ (b + c)) & 0x80) == 0) // Same sign
                && (((a ^ sum) & 0x80) != 0)) // Different sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, (sum > 0xFF) ? true : false); // Set if there is a carry into bit 8

            // Undocumented Flags
            SetFlag(Flags.X, ((((byte)sum) & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((((byte)sum) & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return (byte)sum;
        }

        private ushort Add16(ushort a, ushort b, byte c = 0)
        {
            var sum = a + b + c;

            SetFlag(Flags.N, false);
            //SetFlag(Flags.Z, (ushort)sum == 0 ? true : false);

            var loA = (byte)(a & 0xFF);
            var loB = (byte)(b & 0xFF);
            var hiA = (byte)((a & 0xFF00) >> 8);
            var hiB = (byte)((b & 0xFF00) >> 8);

            if ((loA + loB + c) > 0xFF) hiB++;

            SetFlag(Flags.H, ((hiA & 0x0F) + (hiB & 0x0F) > 0xF) ? true : false);
            SetFlag(Flags.C, (sum > 0xFFFF) ? true : false); // Set if there is a carry into bit 15

            // Undocumented Flags - from high byte
            SetFlag(Flags.X, ((sum & 0x0800) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((sum & 0x2000) > 0) ? true : false); //Copy of bit 5

            MEMPTR = (ushort)(a + 1);

            SetQ();

            return (ushort)sum;
        }

        private ushort Add16WithCarry(ushort a, ushort b, byte c = 0)
        {
            var sum = a + b + c;

            SetFlag(Flags.N, false);
            SetFlag(Flags.Z, (ushort)sum == 0 ? true : false);
            SetFlag(Flags.S, (ushort)(sum & 0b1000_0000_0000_0000) > 0);

            var loA = (byte)(a & 0xFF);
            var loB = (byte)(b & 0xFF);
            var hiA = (byte)((a & 0xFF00) >> 8);
            var hiB = (byte)((b & 0xFF00) >> 8);

            if ((loA + loB + c) > 0xFF) hiB++;

            SetFlag(Flags.H, ((hiA & 0x0F) + (hiB & 0x0F) > 0xF) ? true : false);
            SetFlag(Flags.C, (sum > 0xFFFF) ? true : false); // Set if there is a carry into bit 15

            // Overflow flag
            if (((hiA ^ hiB) & 0x80) == 0 // Same sign
                && (((hiA ^ (hiA + hiB)) & 0x80) != 0)) // Different sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            // Undocumented Flags - from high byte
            SetFlag(Flags.X, ((sum & 0x0800) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((sum & 0x2000) > 0) ? true : false); //Copy of bit 5

            MEMPTR = (ushort)(a + 1);

            SetQ();

            return (ushort)sum;
        }

        private byte Sub8(byte a, byte b, byte c = 0)
        {
            var diff = a - b - c;

            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, ((byte)diff == 0) ? true : false);
            SetFlag(Flags.S, ((((byte)diff) & 0x80) > 0) ? true : false);
            SetFlag(Flags.H, ((a & 0x0F) < ((b + c) & 0x0F)) ? true : false);

            // Overflow flag
            if ((((a ^ (b + c)) & 0x80) != 0)              // Different sign
                && ((((b + c) ^ ((byte)diff)) & 0x80) == 0)) // Same sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, (diff < 0) ? true : false); // Set if there is not a borrow from bit 8

            // Undocumented Flags
            SetFlag(Flags.X, ((((ushort)diff) & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((((ushort)diff) & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return (byte)diff;
        }

        private ushort Sub16(ushort a, ushort b, byte c = 0)
        {
            var diff = a - b - c;

            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, ((ushort)diff == 0) ? true : false);
            SetFlag(Flags.S, ((((ushort)diff) & 0x8000) > 0) ? true : false);

            // Half-carry
            if ((a & 0xFFF) < (b & 0xFFF) + c)
            {
                SetFlag(Flags.H, true);
            }
            else
            {
                SetFlag(Flags.H, false);
            }

            // Overflow flag
            if ((((a ^ (b + c)) & 0x8000) != 0)                  // Different sign
                && ((((b + c) ^ ((ushort)diff)) & 0x8000) == 0)) // Same sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, (diff < 0) ? true : false); // Set if there is not a borrow from bit 15

            // Undocumented Flags - from high byte
            SetFlag(Flags.X, ((diff & 0x0800) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((diff & 0x2000) > 0) ? true : false); //Copy of bit 5

            MEMPTR = (ushort)(a + 1);

            SetQ();

            return (ushort)diff;
        }

        private byte And(byte a, byte b)
        {
            var result = (byte)(a & b);

            SetFlag(Flags.N, false);
            SetFlag(Flags.C, false);
            SetFlag(Flags.S, (result & 0x80) > 0);
            SetFlag(Flags.Z, result == 0x00);
            SetFlag(Flags.P, Parity(result));
            SetFlag(Flags.H, true);

            // Undocumented Flags
            SetFlag(Flags.X, ((result & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((result & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return result;
        }

        private byte Or(byte a, byte b)
        {
            var result = (byte)(a | b);

            SetFlag(Flags.N, false);
            SetFlag(Flags.C, false);
            SetFlag(Flags.S, (result & 0x80) > 0);
            SetFlag(Flags.Z, result == 0x00);
            SetFlag(Flags.P, Parity(result));
            SetFlag(Flags.H, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((result & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((result & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return result;
        }

        private byte Xor(byte a, byte b)
        {
            var result = (byte)(a ^ b);

            SetFlag(Flags.N, false);
            SetFlag(Flags.C, false);
            SetFlag(Flags.S, (result & 0x80) > 0);
            SetFlag(Flags.Z, result == 0x00);
            SetFlag(Flags.P, Parity(result));
            SetFlag(Flags.H, false);

            // Undocumented Flags
            SetFlag(Flags.X, ((result & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((result & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();

            return result;
        }

        private void SetIncFlags(byte val, byte incVal)
        {
            SetFlag(Flags.N, false);
            SetFlag(Flags.S, (sbyte)incVal < 0);
            SetFlag(Flags.P, val == 0x7F);
            SetFlag(Flags.Z, incVal == 0);
            SetFlag(Flags.H, ((val & 0x0F) + (0x01 & 0x0F) > 0xF) ? true : false);

            // Undocumented Flags
            SetFlag(Flags.X, ((incVal & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((incVal & 0x20) > 0) ? true : false); //Copy of bit 5
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
            SetFlag(Flags.X, ((decVal & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((decVal & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();
        }

        private void SetComparisonFlags(byte n, int diff)
        {
            SetFlag(Flags.N, true);
            SetFlag(Flags.Z, diff == 0);
            SetFlag(Flags.S, (byte)diff > 0x7f);

            SetFlag(Flags.H, ((A & 0x0F) < (n & 0x0F)) ? true : false);

            // Overflow flag
            if ((((A ^ n) & 0x80) != 0)              // Different sign
                && (((n ^ ((byte)diff)) & 0x80) == 0)) // Same sign
            {
                SetFlag(Flags.P, true);
            }
            else
            {
                SetFlag(Flags.P, false);
            }

            SetFlag(Flags.C, (n > A) ? true : false);

            // Undocumented Flags
            SetFlag(Flags.X, ((((byte)n) & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((((byte)n) & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();
        }

        private void SetRotateFlags(byte n)
        {
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);
            SetFlag(Flags.Z, n == 0);
            SetFlag(Flags.S, n >= 0x80);
            SetFlag(Flags.P, Parity(n));

            // Undocumented Flags
            SetFlag(Flags.X, ((n & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((n & 0x20) > 0) ? true : false); //Copy of bit 5
            SetQ();
        }

        private void SetShiftArithmeticFlags(byte n)
        {
            SetFlag(Flags.S, n >= 0x80);
            SetFlag(Flags.Z, n == 0);
            SetFlag(Flags.H, false);
            SetFlag(Flags.P, Parity(n));
            SetFlag(Flags.N, false);

            // Undocumented flags
            SetFlag(Flags.X, ((n & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((n & 0x20) > 0) ? true : false); //Copy of bit 5

            SetQ();
        }

        private void SetShiftRightLogicalFlags(byte n)
        {
            SetFlag(Flags.Z, n == 0);
            SetFlag(Flags.P, Parity(n));
            SetFlag(Flags.H, false);
            SetFlag(Flags.N, false);
            SetFlag(Flags.S, false);

            // Undocumented flags
            SetFlag(Flags.X, ((n & 0x08) > 0) ? true : false); //Copy of bit 3
            SetFlag(Flags.U, ((n & 0x20) > 0) ? true : false); //Copy of bit 5

            SetQ();
        }

        private static bool Parity(ushort res)
        {
            var retVal = true;

            while (res > 0)
            {
                if ((res & 0x01) == 1)
                {
                    retVal = !retVal;
                }

                res = (byte)(res >> 1);
            }

            return retVal;
        }

        private byte ReadFromRegister(int src)
        {
            switch (src)
            {
                case 0: return B;
                case 1: return C;
                case 2: return D;
                case 3: return E;
                case 4: return H;
                case 5: return L;
                case 6: return (byte)F;
                case 7: return A;
                default: return 0x00;
            };
        }

        private ushort ReadFromRegisterPair(int src, ushort self)
        {
            switch (src)
            {
                case 0: return BC;
                case 1: return DE;
                case 2: return self;
                case 3: return SP;
                default: return 0x0000;
            }
        }

        private void WriteToRegisterPair(int dest, ushort n)
        {
            switch (dest)
            {
                case 0:
                    B = (byte)((n & 0xFF00) >> 8);
                    C = (byte)(n & 0x00FF);
                    break;
                case 1:
                    D = (byte)((n & 0xFF00) >> 8);
                    E = (byte)(n & 0x00FF);
                    break;
                case 2:
                    H = (byte)((n & 0xFF00) >> 8);
                    L = (byte)(n & 0x00FF);
                    break;
                case 3:
                    SP = n;
                    break;
            }
        }

        private void IncRegisterPair(ushort val, int dest, int inc = 1)
        {
            val = (ushort)(val + inc);
            WriteToRegisterPair(dest, val);
        }

        private void AssignToRegister(int dest, byte n)
        {
            switch (dest)
            {
                case 0:
                    B = n;
                    break;
                case 1:
                    C = n;
                    break;
                case 2:
                    D = n;
                    break;
                case 3:
                    E = n;
                    break;
                case 4:
                    H = n;
                    break;
                case 5:
                    L = n;
                    break;
                case 6:
                    F = (Flags)n;
                    break;
                case 7:
                    A = n;
                    break;
            }
        }

        private bool EvaluateCC(int cc)
        {
            switch (cc)
            {
                case 0: return !CheckFlag(Flags.Z);
                case 1: return CheckFlag(Flags.Z);
                case 2: return !CheckFlag(Flags.C);
                case 3: return CheckFlag(Flags.C);
                case 4: return !CheckFlag(Flags.P);
                case 5: return CheckFlag(Flags.P);
                case 6: return !CheckFlag(Flags.S);
                case 7: return CheckFlag(Flags.S);
                default: return false;
            };
        }

        private ushort GetPageZeroAddress(byte value)
        {
            switch (value)
            {
                case 0:
                    return 0x0000;
                case 1:
                    return 0x0008;
                case 2:
                    return 0x0010;
                case 3:
                    return 0x0018;
                case 4:
                    return 0x0020;
                case 5:
                    return 0x0028;
                case 6:
                    return 0x0030;
                case 7:
                    return 0x0038;
                default:
                    return 0x0000;
            }
        }
    }
}
