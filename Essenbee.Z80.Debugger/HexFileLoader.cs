using System;
using System.Collections.Generic;
using System.IO;

namespace Essenbee.Z80.Debugger
{
    public static class HexFileLoader
    {
        public static (byte[], ushort) Read(string filePath, byte[] RAM)
        {
            var lines = File.ReadAllLines(filePath);
            ushort initialMemoryLocation = 0;
            var lineNo = 0;

            foreach (var line in lines)
            {
                if (line.Equals(":00000001FF", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }

                lineNo++;

                var dataLength = Convert.ToInt32(line[1..3], 16);
                var startAddr = (ushort)Convert.ToInt32(line[3..7], 16);
                var recType = line[7..9];

                if (recType == "00")
                {
                    if (lineNo == 1) initialMemoryLocation = startAddr;

                    // Data record
                    var dataEnd = (2 * dataLength) + 9;
                    var data = line[9..dataEnd];
                    var dataBytes = new List<byte>();

                    for (int i = 0; i < dataLength * 2; i++)
                    {
                        if (i == 0 || i % 2 == 0)
                        {
                            dataBytes.Add((byte)Convert.ToInt32(data.Substring(i, 2), 16));
                        }
                    }

                    foreach (var datum in dataBytes)
                    {
                        RAM[startAddr++] = datum;
                    }
                }
            }

            return (RAM, initialMemoryLocation);
        }
    }
}
