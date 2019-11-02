using Essenbee.Z80.Tests.Classes;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class HexFileReaderTests
    {
        [Fact]
        private void ReadSimpleHexFileWithOnlySingleDataRecord()
        {
            var fakeBus = A.Fake<IBus>();

            // Routine #1 - 58 T-Cycles
            // .ORG   0080h
            //
            // LD A,05h
            // LD   B,0Ah
            // ADD A, B
            // ADD A, A
            // LD C,0Fh
            // SUB C
            // LD H,08h
            // LD   L,0FFh
            // LD(HL),A
            // NOP

            var ram = HexFileReader.Read("../../../HexFiles/Arithmetic1.hex");

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            for (int i = 0; i < 10; i++)
            {
                cpu.Step();
            }

            Assert.Equal(0x0F, ram[0x08FF]);

            void UpdateMemory(ushort addr, byte data)
            {
                ram[addr] = data;
            }
        }
    }
}
