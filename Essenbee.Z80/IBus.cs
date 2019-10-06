using System;
using System.Collections.Generic;
using System.Text;

namespace Essenbee.Z80
{
    public interface IBus
    {
        byte Read(ushort addr, bool readOnly = false);
        void Write(ushort addr, byte data)
    }
}
