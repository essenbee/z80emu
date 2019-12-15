using FakeItEasy;
using System.Collections.Generic;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class InputOutputShould
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
        private void ReadBtyeFromPortForINA()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDB }, // IN A,(&01)
                { 0x0081, 0x01 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x0000, 0x00 },
                { 0x0001, 0x7B },
                { 0x0002, 0x00 },
                { 0x0003, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.ReadPeripheral(A<ushort>._))
                .ReturnsLazily((ushort addr) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            cpu.Step();

            Assert.Equal(0x7B, cpu.A);

            FlagsUnchanged(cpu);
        }
    }
}
