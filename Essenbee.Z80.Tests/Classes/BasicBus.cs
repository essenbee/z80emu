using System;
using System.Collections.Generic;

namespace Essenbee.Z80.Tests.Classes
{
    public class BasicBus : IBus
    {
        private byte[] _memory;
        public BasicBus(int RAMSize)
        {
            _memory = new byte[RAMSize * 1024];
        }

        public BasicBus(byte[] ram)
        {
            _memory = ram;
        }

        public IReadOnlyCollection<byte> RAM
        {
            get => _memory;
        }

        public byte Read(ushort addr, bool ro = false)
        {
            return _memory[addr];
        }

        public byte ReadPeripheral(ushort port)
        {
            // Testing code only
            var r = (byte)(port >> 8);
            return r;
        }

        public void Write(ushort addr, byte data)
        {
            _memory[addr] = data;
        }

        public void WritePeripheral(ushort port, byte data)
        {
            // Testing code only
            _memory[port] = data;
        }
    }
}
