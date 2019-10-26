using System;
using System.Collections.Generic;
using System.IO;

namespace Essenbee.Z80.Tests.Classes
{
    public class HexFileReader
    {
        public byte[] Read(string filePath)
        {
            var RAM = new byte[48 * 1024];
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (line.Equals(":00000001FF"))
                {
                    break;
                }

                var dataLength = Convert.ToInt32(line[1..3], 16);
                var startAddr = (ushort)Convert.ToInt32(line[3..7], 16);
                var recType = line[7..9];

                if (recType == "00")
                {
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

                if (recType == "02")
                {
                    // ToDo: Extended segment address record
                }

                if (recType == "04")
                {
                    // ToDo: Extended linear address record
                }

                if (recType == "05")
                {
                    // ToDo: Start linear address record
                }
            }

            return RAM;
        }
    }
}
