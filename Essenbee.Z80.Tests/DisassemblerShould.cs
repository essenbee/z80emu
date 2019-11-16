using Essenbee.Z80.Tests.Classes;
using FakeItEasy;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class DisassemblerShould
    {
        [Fact]
        private void DisassembleArithmetic1HexFileCorrectly()
        {
            var fakeBus = A.Fake<IBus>();

            var expectedDisassembly = @"0080    LD A,&05
0082    LD B,&0A
0084    ADD A,B
0085    ADD A,A
0086    LD C,&0F
0088    SUB A,C
0089    LD H,&08
008B    LD L,&FF
008D    LD (HL),A
008E    NOP
";

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

            var expectedDisassembly = @"8000    LD BC,&0015
8003    LD B,&08
8005    LD DE,&002A
8008    LD D,&00
800A    LD HL,&0000
800D    SRL C
800F    JR C,$+3
8011    ADD HL,DE
8012    SLA E
8014    RL D
8016    DEC B
8017    JP NZ,&800D
";

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);


            var cpu = new Z80() { A = 0x00, PC = 0x8000 };
            cpu.ConnectToBus(fakeBus);
            var disassembledCode = cpu.Disassemble(0x8000, 0x8017);

            Assert.Equal(expectedDisassembly, disassembledCode);
        }
    }
}
