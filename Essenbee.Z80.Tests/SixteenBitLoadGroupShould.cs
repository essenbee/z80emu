using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class SixteenBitLoadGroupShould
    {
        // ===============================
        // Note: the Z80 is little-endian.
        // ===============================

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
        public void LoadBCwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x01 },
                { 0x0081, 0xCC },
                { 0x0082, 0xAA },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.BC);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadDEwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x11 },
                { 0x0081, 0xCC },
                { 0x0082, 0xAA },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, D = 0x00, E = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.DE);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadHLwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x21 },
                { 0x0081, 0xCC },
                { 0x0082, 0xAA },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.HL);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadSPwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x31 },
                { 0x0081, 0xCC },
                { 0x0082, 0xAA },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, SP = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadIXwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x21 },
                { 0x0082, 0xCC },
                { 0x0083, 0xAA },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, IX = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.IX);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadIYwithNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x21 },
                { 0x0082, 0xCC },
                { 0x0083, 0xAA },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, IY = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.IY);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadHLwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x2A },
                { 0x0081, 0xFF },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0xCC }, // (nn)
                { 0x0900, 0xAA }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.HL);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadBCwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x4B },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0xCC }, // (nn)
                { 0x0900, 0xAA }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, B = 0x00, C = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.BC);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadDEwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x5B },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0xCC }, // (nn)
                { 0x0900, 0xAA }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, D = 0x00, E = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.DE);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadHLwith16BitOperandPointedToByAddressNN2()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x6B },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0xCC }, // (nn)
                { 0x0900, 0xAA }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.HL);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadSPwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x7B },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0xCC }, // (nn)
                { 0x0900, 0xAA }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, SP = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }
    }
}
