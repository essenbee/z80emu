using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Essenbee.Z80.Z80;

namespace Essenbee.Z80.Tests
{
    public class ExchangeShould
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
        private void SwapDEandHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xEB }, // EX DE,HL
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { D = 0x11, E = 0x22, H = 0x33, L = 0x44, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x3344, cpu.DE);
            Assert.Equal(0x1122, cpu.HL);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void SwapAFandAFPrime()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x08 }, // EX AF,AF'
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x11, F = (Flags)0x22, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0000, cpu.AF);
            Assert.Equal(0x1122, cpu.AF1);
            FlagsUnchanged(cpu);
        }

        [Fact]
        private void SwapRegistersWithEXX()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD9 }, // EXX
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { B = 0x11, C = 0x22, D = 0x12, E = 0x23, H = 0x14, L = 0x24, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x0000, cpu.BC);
            Assert.Equal(0x1122, cpu.BC1);
            Assert.Equal(0x0000, cpu.DE);
            Assert.Equal(0x1223, cpu.DE1);
            Assert.Equal(0x0000, cpu.HL);
            Assert.Equal(0x1424, cpu.HL1);

            FlagsUnchanged(cpu);
        }
    }
}
