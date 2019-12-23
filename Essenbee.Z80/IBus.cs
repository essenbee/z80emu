using System.Collections.Generic;

namespace Essenbee.Z80
{
    public interface IBus
    {
        bool Interrupt { get; set; }
        bool NonMaskableInterrupt { get; set; }
        IEnumerable<byte> Data { get; set; }
        IReadOnlyCollection<byte> RAM { get; }
        byte Read(ushort addr, bool ro = false);
        void Write(ushort addr, byte data);
        byte ReadPeripheral(ushort port);
        void WritePeripheral(ushort port, byte data);
    }
}
