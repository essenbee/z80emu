using System;
using System.Collections.Generic;

namespace Essenbee.Z80.Spectrum48
{
    public class SimpleBus : IBus
    {
        public bool Interrupt { get; set; }
        public bool NonMaskableInterrupt { get; set; }
        public IList<byte> Data { get; set; } = new List<byte>();

        private byte[] _memory;

        public SimpleBus(byte[] ram)
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
            Console.WriteLine();
            Console.WriteLine($"IN 0x{port:X4}");

            return 0;
        }

        public void Write(ushort addr, byte data)
        {
            _memory[addr] = data;
        }

        public void WritePeripheral(ushort port, byte data)
        {
            Console.WriteLine();
            Console.WriteLine($"OUT 0x{port:X4}, 0x{data:X2}");
        }
    }
}
