using System.Collections.Generic;

namespace Essenbee.Z80
{
    public interface IBus
    {
        IReadOnlyCollection<byte> RAM { get; }
        byte Read(ushort addr, bool ro = false);
        void Write(ushort addr, byte data);
        byte ReadPeripheral(byte port);
        void WritePeripheral(byte port, byte data);
    }
}
