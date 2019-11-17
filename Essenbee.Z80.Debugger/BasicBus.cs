using System;

namespace Essenbee.Z80.Debugger
{
    public class BasicBus : IBus
    {
        private byte[] RAM;
        public BasicBus(int RAMSize)
        {
            RAM = new byte[RAMSize * 1024];
        }

        public BasicBus(byte[] ram)
        {
            RAM = ram;
        }

        public byte Read(ushort addr, bool ro = false)
        {
            return RAM[addr];
        }

        public byte ReadPeripheral(byte port)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort addr, byte data)
        {
            RAM[addr] = data;
        }

        public void WritePeripheral(byte port, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
