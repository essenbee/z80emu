using Essenbee.Z80.Tests.Classes;
using FakeItEasy;
using System.Collections.Generic;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class DisassemblerShould
    {
        [Fact]
        private void DisassembleArithmetic1HexFileCorrectly()
        {
            var fakeBus = A.Fake<IBus>();

            var expectedDisassembly = new Dictionary<ushort, string>
            {   { 0x0080, "LD A,&05" },
                { 0x0082, "LD B,&0A" },
                { 0x0084, "ADD A,B" },
                { 0x0085, "ADD A,A" },
                { 0x0086, "LD C,&0F" },
                { 0x0088, "SUB A,C" },
                { 0x0089, "LD H,&08" },
                { 0x008B, "LD L,&FF" },
                { 0x008D, "LD (HL),A" },
                { 0x008E, "NOP" },
            };

            var ram = HexFileReader.Read("../../../HexFiles/Arithmetic1.hex");

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            var disassembledCode = cpu.Disassemble(0x0080, 0x008E);

            Assert.Equal(expectedDisassembly, disassembledCode);
        }

        [Fact]
        private void DisassembleMultiplicationHexFileCorrectly()
        {
            var fakeBus = A.Fake<IBus>();
            var ram = HexFileReader.Read("../../../HexFiles/Multiplication.hex");

            var expectedDisassembly = new Dictionary<ushort, string>
            {   { 0x8000, "LD BC,&0015" },
                { 0x8003, "LD B,&08" },
                { 0x8005, "LD DE,&002A" },
                { 0x8008, "LD D,&00" },
                { 0x800A, "LD HL,&0000" },
                { 0x800D, "SRL C" },
                { 0x800F, "JR C,$+3" },
                { 0x8011, "ADD HL,DE" },
                { 0x8012, "SLA E" },
                { 0x8014, "RL D" },
                { 0x8016, "DEC B" },
                { 0x8017, "JP NZ,&800D" },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);


            var cpu = new Z80() { A = 0x00, PC = 0x8000 };
            cpu.ConnectToBus(fakeBus);
            var disassembledCode = cpu.Disassemble(0x8000, 0x8017);

            Assert.Equal(expectedDisassembly, disassembledCode);
        }

        [Fact]
        private void DisassembleDDCBandFDCBOpcodesCorrectly()
        {
            var fakeBus = A.Fake<IBus>();
            var ram = HexFileReader.Read("../../../HexFiles/TestDDCBandFDCB.hex");

            var expectedDisassembly = new Dictionary<ushort, string>
            {   { 0x8000, "RL (IX+2)" },
                { 0x8004, "RL (IY-3)" },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);


            var cpu = new Z80() { A = 0x00, PC = 0x8000 };
            cpu.ConnectToBus(fakeBus);
            var disassembledCode = cpu.Disassemble(0x8000, 0x8007);

            Assert.Equal(expectedDisassembly, disassembledCode);
        }
    }
}
