using System;
using System.Collections.Generic;

namespace Essenbee.Z80.Debugger
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

        public byte ReadPeripheral(byte port)
        {
            throw new NotImplementedException();
        }

        public void Write(ushort addr, byte data)
        {
            _memory[addr] = data;
        }

        public void WritePeripheral(byte port, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
