using Essenbee.Z80.Tests.Classes;
using FakeItEasy;
using System;
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
        private void PassAllValidationTests()
        {
            var tester = new FuseTester();
            var results = tester.RunTests();

            Debug.WriteLine($"Passing tests = {results.Passing.Count}");
            Debug.WriteLine($"Failing tests = {results.Failing.Count}");
            Debug.WriteLine($"Opcodes not implemented = {results.NotImplemented.Count}");

            Assert.Empty(results.Failing);
        }

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

        [Fact]
        private void ExecuteEightBitMultiplicationRoutineSuccessfully()
        {
            var fakeBus = A.Fake<IBus>();

            //` Arithmetic Test Routine #2
            //` Filename: Multiplication.hex
            //`
            //` 8000                          .ORG   8000h
            //`
            //`8000   01 15 00               LD BC,21
            //`8003   06 08                  LD B,8
            //`8005   11 2A 00               LD DE,42
            //`8008   16 00                  LD D,0
            //`800A   21 00 00               LD HL,0
            //`800D   CB 39         MULTI:   SRL C; LSB in Carry Flag
            //`800F   30 01                  JR NC, NOADD
            //`8011   19                     ADD HL, DE
            //`8012   CB 23        NOADD:    SLA E
            //`8014   CB 12                  RL D
            //`8016   05                     DEC B
            //`8017   C2 0D 80               JP NZ, MULTI

            var ram = HexFileReader.Read("../../../HexFiles/Multiplication.hex");

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, H = 0x00, L = 0x00, PC = 0x8000 };
            cpu.ConnectToBus(fakeBus);

            while (cpu.PC < 0x8020)
            {
                cpu.Step();
            }

            Assert.Equal(0x0372, cpu.HL);
        }


        [Fact]
        private void ExecuteEightBitMultiplication2RoutineSuccessfully()
        {
            var fakeBus = A.Fake<IBus>();

            //` Arithmetic Test Routine #3
            //` Filename: Multiplication2.hex
            //`
            //` 8000                          .ORG   8000h
            //`
            //`8000   01 15 00               LD BC,21
            //`8003   06 08                  LD B,8
            //`8005   11 2A 00               LD DE,42
            //`8008   16 00                  LD D,0
            //`800A   21 00 00               LD HL,0
            //`800D   CB 39         MULTI:   SRL C; LSB in Carry Flag
            //`800F   30 01                  JR NC, NOADD
            //`8011   19                     ADD HL, DE
            //`8012   CB 23        NOADD:    SLA E
            //`8014   CB 12                  RL D
            //`8016   10 F5                  DJNZ   MULTI

            var ram = HexFileReader.Read("../../../HexFiles/Multiplication2.hex");

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => ram[addr]);

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, H = 0x00, L = 0x00, PC = 0x8000 };
            cpu.ConnectToBus(fakeBus);

            while (cpu.PC < 0x8018)
            {
                cpu.Step();
            }

            Assert.Equal(0x0372, cpu.HL);
        }
    }
}
