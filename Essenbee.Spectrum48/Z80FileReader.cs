using System;
using System.IO;

namespace Essenbee.Z80.Spectrum48
{
    // Based on code by Arjun Nair
    public static class Z80FileReader
    {
        private const int Size8K = 8192;
        private const int Size48K = 49152;
        private const int Version1HeaderLength = 30;

        public static Z80Snapshot LoadZ80File(string filename)
        {
            Z80Snapshot snapshot;

            using (var fs = new FileStream(filename, FileMode.Open))
            {
                snapshot = LoadZ80Snapshot(fs);
            }

            return snapshot;
        }

        private static Z80Snapshot LoadZ80Snapshot(Stream fs)
        {
            var snapshot = new Z80Snapshot();

            //        Offset Length   Description
            //        ----------------------------
            //        0       1       A register
            //        1       1       F register
            //        2       2       BC register pair
            //        4       2       HL register pair
            //        6       2       Program counter
            //        8       2       Stack pointer
            //        10      1       Interrupt register
            //        11      1       Refresh register(Bit 7 is not significant!)
            //        12      1       Bit 0    : Bit 7 of the R - register
            //                        Bit 1 - 3: Border colour
            //                        Bit 4    : 1 = Basic SamRom switched in
            //                        Bit 5    : 1 = Block of data is compressed
            //                        Bit 6 - 7: No meaning
            //        13      2       DE register pair
            //        15      2       BC' register pair
            //        17      2       DE' register pair
            //        19      2       HL' register pair
            //        21      1       A' register
            //        22      1       F' register
            //        23      2       IY register
            //        25      2       IX register
            //        27      1       Interrupt flipflop 1, 0 = DI, otherwise EI
            //        28      1       IFF2
            //        29      1       Bit 0 - 1: Interrupt mode(0, 1 or 2)
            //                        Bit 2    : 1 = Issue 2 emulation
            //                        Bit 3    : 1 = Double interrupt frequency
            //                        Bit 4 - 5: 1 = High video synchronization
            //                                   3 = Low video synchronization
            //                                   0,2 = Normal
            //                        Bit 6 - 7: 0 = Cursor / Protek / AGF joystick
            //                                   1 = Kempston joystick
            //                                   2 = Sinclair 2 Left joystick(or user
            //                                       defined, for version 3.z80 files)
            //                                   3 = Sinclair 2 Right joystick

            using (var r = new BinaryReader(fs))
            {
                var readCount = (int)fs.Length;
                var buffer = new byte[readCount];
                var bytes = r.Read(buffer, 0, readCount);

                if (bytes == 0)
                {
                    return null;
                }

                // .Z80 File Version 1 Header
                snapshot.AF = buffer[0] << 8;
                snapshot.AF |= buffer[1];
                snapshot.BC = buffer[2] | (buffer[3] << 8);
                snapshot.HL = buffer[4] | (buffer[5] << 8);
                snapshot.PC = buffer[6] | (buffer[7] << 8);
                snapshot.SP = buffer[8] | (buffer[9] << 8);
                snapshot.I = buffer[10];
                snapshot.R = buffer[11];

                var byte12 = buffer[12];

                // If 255, has to be regarded as being 1 for compatability
                if (byte12 == 255)
                {
                    byte12 = 1;
                }

                snapshot.R |= (byte)((byte12 & 0x01) << 7);
                snapshot.Border = (byte)((byte12 >> 1) & 0x07);
                var isCompressed = (byte12 & 0x20) != 0;

                // Little-endian values
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

                // Memory Snapshot
                for (var i = 0; i < 16; i++)
                {
                    snapshot.RAMBank[i] = new byte[Size8K];
                }

                if (snapshot.PC == 0)
                {
                    // Version 2 Header (additional)
                    var headerLength = buffer[Version1HeaderLength];
                    snapshot.PC = buffer[32] | (buffer[33] << 8);
                    snapshot.Type = GetSnapshotType(buffer, headerLength);
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

                    var bank = new byte[Size8K * 2];

                    while (counter < buffer.Length)
                    {
                        var dataLength = buffer[counter] | (buffer[counter + 1] << 8);
                        counter += 2;

                        if (counter >= buffer.Length)
                        {
                            break;
                        }

                        var page = buffer[counter++];
                        GetPage(buffer, counter, bank, dataLength);
                        counter += (dataLength == 0xFFFF) ? (Size8K * 2) : dataLength;

                        switch (page)
                        {
                            case 0:
                            case 1:
                            case 2:
                                break;
                            case 3:
                                Array.Copy(bank, 0, snapshot.RAMBank[0], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[1], 0, Size8K);
                                break;
                            case 4:
                                if (snapshot.Type > 0)
                                {
                                    Array.Copy(bank, 0, snapshot.RAMBank[2], 0, Size8K);
                                    Array.Copy(bank, Size8K, snapshot.RAMBank[3], 0, Size8K);
                                }
                                else
                                {
                                    // Spectrum 48K
                                    Array.Copy(bank, 0, snapshot.RAMBank[4], 0, Size8K);
                                    Array.Copy(bank, Size8K, snapshot.RAMBank[5], 0, Size8K);
                                }
                                break;
                            case 5:
                                if (snapshot.Type > 0)
                                {
                                    Array.Copy(bank, 0, snapshot.RAMBank[4], 0, Size8K);
                                    Array.Copy(bank, Size8K, snapshot.RAMBank[5], 0, Size8K);
                                }
                                else
                                {
                                    // Spectrum 48K
                                    Array.Copy(bank, 0, snapshot.RAMBank[0], 0, Size8K);
                                    Array.Copy(bank, Size8K, snapshot.RAMBank[1], 0, Size8K);
                                }
                                break;
                            case 6:
                                Array.Copy(bank, 0, snapshot.RAMBank[6], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[7], 0, Size8K);
                                break;
                            case 7:
                                Array.Copy(bank, 0, snapshot.RAMBank[8], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[9], 0, Size8K);
                                break;
                            case 8:
                                // Spectrum 48K and 128K
                                Array.Copy(bank, 0, snapshot.RAMBank[10], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[11], 0, Size8K);
                                break;
                            case 9:
                                Array.Copy(bank, 0, snapshot.RAMBank[12], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[13], 0, Size8K);
                                break;
                            case 10:
                                Array.Copy(bank, 0, snapshot.RAMBank[14], 0, Size8K);
                                Array.Copy(bank, Size8K, snapshot.RAMBank[15], 0, Size8K);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    snapshot.Type = 0;
                    var spectrum48Memory = new byte[Size48K];

                    if (!isCompressed)
                    {
                        Array.Copy(buffer, Version1HeaderLength, spectrum48Memory, 0, Size48K);
                    }
                    else
                    {
                        // The compression method is very simple: repetitions of at least five 
                        // equal bytes are replaced by a four-byte code ED ED xx yy, which stands
                        // for *byte yy repeated xx times*. Only sequences at least 5 long are
                        // thus encoded. An exception is a sequence consisting of EDs; if they are 
                        // encountered, even two EDs are encoded into ED ED 02 ED. Finally, every 
                        // byte directly following a single ED is not taken into a block, e.g. 
                        // ED 6 * 00 is not encoded into ED ED ED 06 00 but into ED 00 ED ED 05 00.
                        // The block is terminated by an end marker: 00 ED ED 00.

                        var byteCounter = Version1HeaderLength;
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

                    Array.Copy(spectrum48Memory, 0, snapshot.RAMBank[10], 0, Size8K);
                    Array.Copy(spectrum48Memory, Size8K, snapshot.RAMBank[11], 0, Size8K);
                    Array.Copy(spectrum48Memory, Size8K * 2, snapshot.RAMBank[4], 0, Size8K);
                    Array.Copy(spectrum48Memory, Size8K * 3, snapshot.RAMBank[5], 0, Size8K);
                    Array.Copy(spectrum48Memory, Size8K * 4, snapshot.RAMBank[0], 0, Size8K);
                    Array.Copy(spectrum48Memory, Size8K * 5, snapshot.RAMBank[1], 0, Size8K);

                    snapshot.Spectrum48 = spectrum48Memory;
                }
            }

            return snapshot;
        }

        private static int GetSnapshotType(byte[] buffer, byte headerLength)
        {
            switch (buffer[34])
            {
                case 0:
                case 1:
                    return 0;
                case 3:
                    return (headerLength == 23) ? 1 : 0;
                case 4:
                case 5:
                case 6:
                    return 1;
                case 7:
                case 8:
                    return 2;
                default:
                    return 3;
            }
        }

        private static void GetPage(byte[] buffer, int counter, byte[] bank, int dataLength)
        {
            if (dataLength == 0xFFFF)
            {
                Array.Copy(buffer, counter, bank, 0, Size8K * 2);
            }
            else
            {
                var blockOffset = counter;
                var startOfMemory = 0;

                while (counter - blockOffset < dataLength)
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
    }
}
