using System;
using System.IO;

namespace Essenbee.Z80.Spectrum48
{
    // Based on code by Arjun Nair
    public static class Z80FileReader
    {
        private static void GetPage(byte[] buffer, int counter, byte[] bank, int dataLength)
        {
            if (dataLength == 0xFFFF)
            {
                Array.Copy(buffer, counter, bank, 0, 16384);
            }
            else
            {
                var blockOffset = counter;
                var startOfMemory = 0;

                while ((counter - blockOffset) < dataLength)
                {
                    var aByte = buffer[counter++];

                    if (aByte == 0xED)
                    {
                        var aByte2 = buffer[counter];

                        if (aByte2 == 0xED)
                        {
                            counter++;
                            var dataSize = buffer[counter++];
                            var data = buffer[counter++];

                            // Is compressed data
                            for (var f = 0; f < dataSize; f++)
                            {
                                bank[startOfMemory++] = data;
                            }

                            continue;
                        }

                        bank[startOfMemory++] = aByte;
                        continue;
                    }
                    else
                    {
                        bank[startOfMemory++] = aByte;
                    }
                }
            }
        }

        public static Z80Snapshot LoadZ80Snapshot(System.IO.Stream fs)
        {
            var snapshot = new Z80Snapshot();

            using (BinaryReader r = new BinaryReader(fs))
            {
                var readCount = (int)fs.Length;
                var buffer = new byte[readCount];
                var bytes = r.Read(buffer, 0, readCount);

                if (bytes == 0)
                {
                    return null;
                }

                snapshot.AF = buffer[0] << 8;
                snapshot.AF |= buffer[1];
                snapshot.BC = buffer[2] | (buffer[3] << 8);
                snapshot.HL = buffer[4] | (buffer[5] << 8);
                snapshot.PC = buffer[6] | (buffer[7] << 8);
                snapshot.SP = buffer[8] | (buffer[9] << 8);
                snapshot.I = buffer[10];
                snapshot.R = buffer[11];

                var byte12 = buffer[12];

                if (byte12 == 255)
                {
                    byte12 = 1;
                }

                snapshot.R |= (byte)((byte12 & 0x01) << 7);
                snapshot.Border = (byte)((byte12 >> 1) & 0x07);
                var isCompressed = (byte12 & 0x20) != 0;

                snapshot.DE = buffer[13] | (buffer[14] << 8);
                snapshot.BC1 = buffer[15] | (buffer[16] << 8);
                snapshot.DE1 = buffer[17] | (buffer[18] << 8);
                snapshot.HL1 = buffer[19] | (buffer[20] << 8);
                snapshot.AF1 = (buffer[21] << 8) | buffer[22];

                snapshot.IY = buffer[23] | (buffer[24] << 8);
                snapshot.IX = buffer[25] | (buffer[26] << 8);

                snapshot.IFF1 = buffer[27] != 0;
                snapshot.IFF2 = buffer[28] != 0;

                var byte29 = buffer[29];
                snapshot.IM = (byte)(byte29 & 0x3);
                snapshot.IsIssue2 = (byte29 & 0x08) != 0;

                for (var i = 0; i < 16; i++)
                {
                    snapshot.RAMBank[i] = new byte[8192];
                }

                if (snapshot.PC == 0)
                {
                    var headerLength = buffer[30];
                    snapshot.PC = buffer[32] | (buffer[33] << 8);

                    switch (buffer[34])
                    {
                        case 0:
                            snapshot.Type = 0;
                            break;
                        case 1:
                            snapshot.Type = 0;
                            break;
                        case 3:
                            snapshot.Type = headerLength == 23 ? 1 : 0;
                            break;
                        case 4:
                            snapshot.Type = 1;
                            break;
                        case 5:
                            snapshot.Type = 1;
                            break;
                        case 6:
                            snapshot.Type = 1;
                            break;
                        case 7:
                            snapshot.Type = 2;
                            break;
                        case 8:
                            snapshot.Type = 2;
                            break;

                        case 9:
                            snapshot.Type = 3;
                            break;
                    }

                    var counter = 32 + headerLength;

                    snapshot.Port7FFD = buffer[35];
                    snapshot.AY48K = (buffer[37] & 0x4) != 0;
                    snapshot.PortFFFD = buffer[38];
                    snapshot.AYRegisters = new byte[16];

                    for (var i = 0; i < 16; i++)
                    {
                        snapshot.AYRegisters[i] = buffer[39 + i];
                    }

                    snapshot.TStates = 0;

                    if (headerLength != 23)
                    {
                        snapshot.TStates = (buffer[55] | (buffer[56] << 8)) * buffer[57];

                        if (headerLength == 55)
                        {
                            snapshot.Port1FFD = buffer[86];
                        }
                    }

                    var _bank = new byte[16384];

                    while (counter < buffer.Length)
                    {
                        var dataLength = buffer[counter] | (buffer[counter + 1] << 8);
                        counter += 2;

                        if (counter >= buffer.Length)
                        {
                            break;
                        }

                        var page = buffer[counter++];
                        GetPage(buffer, counter, _bank, dataLength);
                        counter += (dataLength == 0xffff ? 16384 : dataLength);

                        switch (page)
                        {
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3:
                                Array.Copy(_bank, 0, snapshot.RAMBank[0], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[1], 0, 8192);
                                break;
                            case 4:
                                if (snapshot.Type > 0)
                                {
                                    Array.Copy(_bank, 0, snapshot.RAMBank[2], 0, 8192);
                                    Array.Copy(_bank, 8192, snapshot.RAMBank[3], 0, 8192);
                                }
                                else
                                {
                                    // Spectrum 48K
                                    Array.Copy(_bank, 0, snapshot.RAMBank[4], 0, 8192);
                                    Array.Copy(_bank, 8192, snapshot.RAMBank[5], 0, 8192);
                                }
                                break;
                            case 5:
                                if (snapshot.Type > 0)
                                {
                                    Array.Copy(_bank, 0, snapshot.RAMBank[4], 0, 8192);
                                    Array.Copy(_bank, 8192, snapshot.RAMBank[5], 0, 8192);
                                }
                                else
                                {
                                    // Spectrum 48K
                                    Array.Copy(_bank, 0, snapshot.RAMBank[0], 0, 8192);
                                    Array.Copy(_bank, 8192, snapshot.RAMBank[1], 0, 8192);
                                }
                                break;
                            case 6:
                                Array.Copy(_bank, 0, snapshot.RAMBank[6], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[7], 0, 8192);
                                break;
                            case 7:
                                Array.Copy(_bank, 0, snapshot.RAMBank[8], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[9], 0, 8192);
                                break;
                            case 8:
                                // Spectrum 48K and 128K
                                Array.Copy(_bank, 0, snapshot.RAMBank[10], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[11], 0, 8192);
                                break;
                            case 9:
                                Array.Copy(_bank, 0, snapshot.RAMBank[12], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[13], 0, 8192);
                                break;
                            case 10:
                                Array.Copy(_bank, 0, snapshot.RAMBank[14], 0, 8192);
                                Array.Copy(_bank, 8192, snapshot.RAMBank[15], 0, 8192);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    snapshot.Type = 0;
                    var spectrum48Memory = new byte[49152];

                    if (!isCompressed)
                    {
                        Array.Copy(buffer, 30, spectrum48Memory, 0, 49152);
                    }
                    else
                    {
                        var byteCounter = 30;
                        var memCounter = 0;

                        while (true)
                        {
                            var aByte = buffer[byteCounter++];

                            if (aByte == 0)
                            {
                                var aByte2 = buffer[byteCounter];

                                if (aByte2 == 0xED)
                                {
                                    var aByte3 = buffer[byteCounter + 1];

                                    if (aByte3 == 0xED)
                                    {
                                        var aByte4 = buffer[byteCounter + 2];

                                        if (aByte4 == 0)
                                        {
                                            break;
                                        }
                                    }
                                }

                                spectrum48Memory[memCounter++] = aByte;
                            }
                            else if (aByte == 0xED)
                            {
                                var aByte2 = buffer[byteCounter];

                                if (aByte2 == 0xED)
                                {
                                    byteCounter++;
                                    var dataLength = buffer[byteCounter++];
                                    var data = buffer[byteCounter++];

                                    // Is compressed data
                                    for (var i = 0; i < dataLength; i++)
                                    {
                                        spectrum48Memory[memCounter++] = data;
                                    }

                                    continue;
                                }

                                spectrum48Memory[memCounter++] = aByte;
                                continue;
                            }
                            else
                            {
                                spectrum48Memory[memCounter++] = aByte;
                            }
                        }
                    }

                    Array.Copy(spectrum48Memory, 0, snapshot.RAMBank[10], 0, 8192);
                    Array.Copy(spectrum48Memory, 8192, snapshot.RAMBank[11], 0, 8192);
                    Array.Copy(spectrum48Memory, 8192 * 2, snapshot.RAMBank[4], 0, 8192);
                    Array.Copy(spectrum48Memory, 8192 * 3, snapshot.RAMBank[5], 0, 8192);
                    Array.Copy(spectrum48Memory, 8192 * 4, snapshot.RAMBank[0], 0, 8192);
                    Array.Copy(spectrum48Memory, 8192 * 5, snapshot.RAMBank[1], 0, 8192);

                    snapshot.Spectrum48 = spectrum48Memory;
                }
            }

            return snapshot;
        }

        public static Z80Snapshot LoadZ80File(string filename)
        {
            Z80Snapshot snapshot;

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                snapshot = LoadZ80Snapshot(fs);
            }

            return snapshot;
        }
    }
}
