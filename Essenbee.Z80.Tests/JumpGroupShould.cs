using FakeItEasy;
using System.Collections.Generic;
using Xunit;
using static Essenbee.Z80.Z80;

namespace Essenbee.Z80.Tests
{
    public class JumpGroupShould
    {
        private static void FlagsUnchanged(Z80 cpu)
        {
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void LoadProgramCounterWithAddressForJPNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xC3 }, // JP 0191h
                { 0x0081, 0x91 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 },
                { 0x0191, 0x00 }, // <- jump here
                { 0x0192, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0191, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void JumpWhenCarryFlagSetForJPCCNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x37 }, // SCF
                { 0x0081, 0xDA }, // JP C, 0191h
                { 0x0082, 0x91 },
                { 0x0083, 0x01 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },

                { 0x0190, 0x00 },
                { 0x0191, 0x00 }, // <- jump here
                { 0x0192, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();
            cpu.Step();

            Assert.Equal(0x0191, cpu.PC);
            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void NotJumpWhenCarryFlagNotSetForJPCCNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDA }, // JP C, 0191h
                { 0x0081, 0x91 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 },
                { 0x0191, 0x00 }, // <- jump here
                { 0x0192, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.NotEqual(0x0191, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void JumpWhenOddParityForJPCCNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xE2 }, // JP C, 0191h
                { 0x0081, 0x91 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 },
                { 0x0191, 0x00 }, // <- jump here
                { 0x0192, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, F = (Flags)0b00000000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0191, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void NotJumpWhenEvenParityForJPCCNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xEA }, // JP C, 0191h
                { 0x0081, 0x91 },
                { 0x0082, 0x01 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                { 0x0190, 0x00 },
                { 0x0191, 0x00 }, // <- jump here
                { 0x0192, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, F = (Flags)0b00000100, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0191, cpu.PC);
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void JumpForwardFourForJRPositiveSix()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x18 }, // JR $+6
                { 0x0081, 0x04 }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 }, // <- jump here
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x086, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void JumpBackFourForJRNegativeFour()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x007B, 0x00 },
                { 0x007C, 0x00 }, // <- jump here
                { 0x007D, 0x00 },
                { 0x007E, 0x00 },
                { 0x007F, 0x00 },
                { 0x0080, 0x18 }, // JR $-4
                { 0x0081, 0xFA }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x007C, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void JumpForwardFourForJRCPositiveSix_CarrySet()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x37 }, // SCF
                { 0x0081, 0x38 }, // JR C,$+6
                { 0x0082, 0x04 }, // Assembler with compensate for PC incrementing twice
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 }, // <- jump here
                { 0x0088, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();
            cpu.Step();

            Assert.Equal(0x087, cpu.PC);
            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void DoNotJumpForwardFourForJRCPositiveSix_CarryNotSet()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x38 }, // JR C $+6
                { 0x0081, 0x04 }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x082, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void DoNotJumpForwardFourForJRNCPositiveSix_CarrySet()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x37 }, // SCF
                { 0x0081, 0x30 }, // JR NC,$+6
                { 0x0082, 0x04 }, // Assembler with compensate for PC incrementing twice
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
                { 0x0088, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();
            cpu.Step();

            Assert.Equal(0x083, cpu.PC);
            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void JumpForwardFourForJRNCPositiveSix_CarryNotSet()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x30 }, // JR NC $+6
                { 0x0081, 0x04 }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 }, // <- jump here
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x086, cpu.PC);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void JumpBackFourForDJNZ_WhenNotZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x007C, 0x00 }, // <- jump here
                { 0x007D, 0x00 },
                { 0x007E, 0x00 },
                { 0x007F, 0x00 },
                { 0x0080, 0x10 }, // DJNZ $-4
                { 0x0081, 0xFA }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { B = 0x05, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x07C, cpu.PC);
        }


        [Fact]
        private void NotJumpBackFourForDJNZ_WhenZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x007C, 0x00 }, // <- jump here
                { 0x007D, 0x00 },
                { 0x007E, 0x00 },
                { 0x007F, 0x00 },
                { 0x0080, 0x10 }, // DJNZ $-4
                { 0x0081, 0xFA }, // Assembler with compensate for PC incrementing twice
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
                { 0x0085, 0x00 },
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { B = 0x01, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x082, cpu.PC);
        }
    }
}
