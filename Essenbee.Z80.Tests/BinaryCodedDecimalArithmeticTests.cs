using FakeItEasy;
using System.Collections.Generic;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class BinaryCodedDecimalArithmeticTests
    {
        [Fact]
        private void EightBitBCDAddition()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x3E }, // LD A,15h    0001 0101
                { 0x0081, 0x15 },
                { 0x0082, 0x06 }, // LD B,27h    0010 0111
                { 0x0083, 0x27 },
                { 0x0084, 0x80 }, // ADD A,B
                { 0x0085, 0x27 }, // DAA
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
                { 0x0088, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            for (int i = 0; i < 5; i++)
            {
                cpu.Step();
            }

            // BCD 42 is the answer... 0100 0010
            Assert.Equal(0x04, cpu.A >> 4);   // Tens digit is 4
            Assert.Equal(0x02, cpu.A & 0x0F); // Ones digit is 2

            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);  // Even parity
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.True((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }

        [Fact]
        private void EightBitBCDSubtractionNegativeResult()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x3E }, // LD A,15h    0001 0101
                { 0x0081, 0x15 },
                { 0x0082, 0x06 }, // LD B,27h    0010 0111
                { 0x0083, 0x27 },
                { 0x0084, 0x90 }, // SUB A,B
                { 0x0085, 0x27 }, // DAA
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
                { 0x0088, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            for (int i = 0; i < 5; i++)
            {
                cpu.Step();
            }

            // BCD -12 is the answer (represented as the 10s complement form, 88) with sign bit set
            Assert.Equal(0x08, cpu.A >> 4);
            Assert.Equal(0x08, cpu.A & 0x0F);

            // Reverse the tens complement form...
            Assert.True((cpu.F & Z80.Flags.S) == Z80.Flags.S); // Sign is -
            Assert.Equal(0x01, 9 - (cpu.A >> 4));              // Tens digit is 1
            Assert.Equal(0x02, 9 - (cpu.A & 0x0F) + 1);        // Ones digit is 2

            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);  // Subtraction
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);  // Even parity
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
        }

        [Fact]
        private void EightBitBCDSubtraction()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x3E }, // LD A,27h    0010 0111
                { 0x0081, 0x27 },
                { 0x0082, 0x06 }, // LD B,15h    0001 0101
                { 0x0083, 0x15 },
                { 0x0084, 0x90 }, // SUB A,B
                { 0x0085, 0x27 }, // DAA
                { 0x0086, 0x00 },
                { 0x0087, 0x00 },
                { 0x0088, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);

            for (int i = 0; i < 5; i++)
            {
                cpu.Step();
            }

            // BCD 12 is the answer... 00001 0010
            Assert.Equal(0x01, cpu.A >> 4);   // Tens digit is 1
            Assert.Equal(0x02, cpu.A & 0x0F); // Ones digit is 2

            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);  // Subtraction
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);  // Even parity
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }
    }
}
