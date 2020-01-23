namespace Essenbee.Z80.Spectrum48
{
    public class Z80Snapshot
    {
        public int Type;
        public byte I;
        public int HL1, DE1, BC1, AF1;
        public int HL, DE, BC, IX, IY;
        public byte R;
        public int AF, SP;
        public byte IM;
        public byte Border;
        public int PC;
        public byte Port7FFD;
        public byte PortFFFD;
        public byte Port1FFD;
        public byte[] AYRegisters;
        public bool IFF1;
        public bool IFF2;
        public bool IsIssue2;
        public bool AY48K;
        public int TStates;
        public byte[][] RAMBank = new byte[16][];
        public byte[] Spectrum48 = new byte[49152];
    }
}