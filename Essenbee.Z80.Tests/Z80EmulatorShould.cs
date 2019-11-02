using Essenbee.Z80.Tests.Classes;
using FakeItEasy;
using System.Diagnostics;
using Xunit;

// ===============================================================
// Online Z80 Assembler: https://www.asm80.com/onepage/asmz80.html
// ===============================================================

namespace Essenbee.Z80.Tests
{
    public class Z80EmulatorShould
    {
        [Fact]
        private void ExecuteArithmeticTestRoutine1Successfully()
        {
            var fakeBus = A.Fake<IBus>();

            //` Arithmetic Test Routine #1 - 10 instructions
            //` Filename: Arithmetic1.hex
            //`
            //` 0080                          .ORG   0080h
            //`
            //` 0080   3E 05                  LD A,05h
            //` 0082   06 0A                  LD B,0Ah
            //` 0084   80                     ADD A,B
            //` 0085   87                     ADD A,A
            //` 0086   0E 0F                  LD C,0Fh
            //` 0088   91                     SUB C
            //` 0089   26 08                  LD H,08h
            //` 008B   2E FF                  LD L,0FFh
            //` 008D   77                     LD (HL),A
            //` 008E   00                     NOP

            var ram = HexFileReader.Read("../../../HexFiles/Arithmetic1.hex");

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            // Run 10 instructions
            for (int i = 0; i < 10; i++)
            {
                cpu.Step();
                Debug.WriteLine($"A = {cpu.A} B = {cpu.B} C = {cpu.C} H = {cpu.H} L = {cpu.L}");
            }
            
            Assert.Equal(0x0F, ram[0x08FF]);

            void UpdateMemory(ushort addr, byte data)
            {
                ram[addr] = data;
            }
        }
    }
}
