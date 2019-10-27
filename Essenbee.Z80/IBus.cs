namespace Essenbee.Z80
{
    public interface IBus
    {
        byte Read(ushort addr, bool readOnly = false);
        void Write(ushort addr, byte data);
        byte ReadPeripheral(byte port);
        void WritePeripheral(byte port, byte data);
    }
}
