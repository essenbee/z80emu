using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Essenbee.Z80.Z80;

namespace Essenbee.Z80.Tests
{
    public class CallAndReturnGroupShould
    {
        [Fact]
        private void PushAndSetProgramCounterForCALL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xCD }, // CALL &0190
                { 0x0081, 0x90 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 }, // <- Subroutine
                { 0x0191, 0x00 },
                { 0x0192, 0x00 },

                { 0x1FFB, 0x00 },
                { 0x1FFC, 0x00 },
                { 0x1FFD, 0x00 },
                { 0x1FFE, 0x00 },
                { 0x1FFF, 0x00 },
                { 0x2000, 0x00 }, // <- SP
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, PC = 0x0080, SP = 0x2000 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0190, cpu.PC);
            Assert.Equal(0x1FFE, cpu.SP);
            Assert.Equal(0x00, program[0x1FFF]);
            Assert.Equal(0x83, program[0x1FFE]);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        private void PushAndSetProgramCounterForCALLCC_GivenZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xCD }, // CALL &0190
                { 0x0081, 0x90 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 }, // <- Subroutine
                { 0x0191, 0x00 },
                { 0x0192, 0x00 },

                { 0x1FFB, 0x00 },
                { 0x1FFC, 0x00 },
                { 0x1FFD, 0x00 },
                { 0x1FFE, 0x00 },
                { 0x1FFF, 0x00 },
                { 0x2000, 0x00 }, // <- SP
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, PC = 0x0080, SP = 0x2000 };
            cpu.ConnectToBus(fakeBus);
            cpu.F = (Flags)0b01000000; // Set Z flag

            cpu.Step();

            Assert.Equal(0x0190, cpu.PC);
            Assert.Equal(0x1FFE, cpu.SP);
            Assert.Equal(0x00, program[0x1FFF]);
            Assert.Equal(0x83, program[0x1FFE]);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        private void DoNothingForCALLCC_GivenNotZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xCC }, // CALL Z, &0190
                { 0x0081, 0x90 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 }, // <- Subroutine
                { 0x0191, 0x00 },
                { 0x0192, 0x00 },

                { 0x1FFB, 0x00 },
                { 0x1FFC, 0x00 },
                { 0x1FFD, 0x00 },
                { 0x1FFE, 0x00 },
                { 0x1FFF, 0x00 },
                { 0x2000, 0x00 }, // <- SP
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, PC = 0x0080, SP = 0x2000 };
            cpu.ConnectToBus(fakeBus);
            cpu.F = (Flags)0b00000000; // Reset Z flag

            cpu.Step();

            Assert.Equal(0x0083, cpu.PC);
            Assert.Equal(0x2000, cpu.SP);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }
    }
}
