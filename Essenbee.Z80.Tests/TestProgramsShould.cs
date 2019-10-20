using FakeItEasy;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class TestProgramsShould
    {
        [Fact]
        private void CompleteSimpleArithmeticRoutine1Successfully()
        {
            var fakeBus = A.Fake<IBus>();

            // Routine #1 - 58 T-Cycles
            // 0080                          .ORG   0080h
            //
            // 0080   3E 05                  LD A,05h
            // 0082   06 0A                  LD   B,0Ah
            // 0084   80                     ADD A, B
            // 0085   87                     ADD A, A
            // 0086   0E 0F                  LD C,0Fh
            // 0088   91                     SUB C
            // 0089   26 08                  LD H,08h
            // 008B   2E FF                  LD   L,0FFh
            // 008D   77                     LD(HL),A
            // 008E   00                     NOP

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x3E }, // LD A, 0x05
                { 0x0081, 0x05 },
                { 0x0082, 0x06 }, // LD B, 0x0A
                { 0x0083, 0x0A },
                { 0x0084, 0x80 }, // ADD A, B
                { 0x0085, 0x87 }, // ADD A, A
                { 0x0086, 0x0E }, // LD C, 0x0F
                { 0x0087, 0x0F },
                { 0x0088, 0x99 }, // SUB C
                { 0x0089, 0x26 }, // LD H, 0x08
                { 0x008A, 0x08 },
                { 0x008B, 0x2E }, // LD L, 0xFF
                { 0x008C, 0xFF },
                { 0x008D, 0x77 }, // LD (HL), A
                { 0x008E, 0x00 }, // NOP
                { 0x008F, 0x00 }, // NOP
                { 0x0090, 0x00 }, // NOP

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // <-- Result stored here (0x0F expected)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            // Run 58 T-cycles = 54 + NOP
            for (int i = 0; i < 58; i++)
            {
                cpu.Tick();
                Debug.WriteLine($"A = {cpu.A} B = {cpu.B} C = {cpu.C} H = {cpu.H} L = {cpu.L}");
            }
            
            Assert.Equal(0x0F, program[0x08FF]);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }
    }
}
