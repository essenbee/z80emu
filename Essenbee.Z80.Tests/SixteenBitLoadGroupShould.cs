using FakeItEasy;
using System.Collections.Generic;
using Xunit;
using static Essenbee.Z80.Z80;

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

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
            cpu.Step();

            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadIXwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x2A },
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

            var cpu = new Z80() { A = 0x00, IX = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xAACC, cpu.IX);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadIYwith16BitOperandPointedToByAddressNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x2A },
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

            var cpu = new Z80() { A = 0x00, IY = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xAACC, cpu.IY);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadNNwith16BitOperandFromHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x22 },
                { 0x0081, 0xFF },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, H = 0xAA, L = 0xCC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromBC()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x43 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, B = 0xAA, C = 0xCC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromDE()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x53 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, D = 0xAA, E = 0xCC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromHL2()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x63 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, H = 0xAA, L = 0xCC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromSP()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x73 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0xAACC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromIX()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x22 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, IX = 0xAACC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadNNwith16BitOperandFromIY()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x22 },
                { 0x0082, 0xFF },
                { 0x0083, 0x08 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (nn)
                { 0x0900, 0x00 }, // (nn + 1)
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, IY = 0xAACC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xCC, program[0x08FF]);
            Assert.Equal(0xAA, program[0x08FF + 1]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadSPwithHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xF9 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, SP = 0x0000, H = 0xAA, L = 0xCC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xAACC, cpu.HL);
            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadSPwithIX()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0xF9 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, SP = 0x0000, IX = 0xAACC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xAACC, cpu.IX);
            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadSPwithIY()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0xF9 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, SP = 0x0000, IY = 0xAACC, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0xAACC, cpu.IY);
            Assert.Equal(0xAACC, cpu.SP);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void PushBC()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xC5 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, B = 0x10, C = 0x11, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushDE()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD5 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, D = 0x10, E = 0x11, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xE5 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, H = 0x10, L = 0x11, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushAF()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xF5 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { SP = 0x0905, A = 0x10, F = (Flags)0x11, PC = 0x0080 };

            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.True((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushIX()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0xE5 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, IX = 0x1011, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushIY()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0xE5 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, IY = 0x1011, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopBC()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xC1 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 }, 
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, B = 0x00, C = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x3355, cpu.BC);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopDE()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD1 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 },
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, D = 0x00, E = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x3355, cpu.DE);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xE1 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 },
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, H = 0x00, L = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x3355, cpu.HL);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopAF()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xF1 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 },
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x33, cpu.A);
            Assert.Equal(0x55, (byte)cpu.F);

            // Condition Flags = 01010101
            Assert.True((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.True((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopIX()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0xE1 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 },
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, IX = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x3355, cpu.IX);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PopIY()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0xE1 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x0FFB, 0x00 },
                { 0x0FFC, 0x00 },
                { 0x0FFD, 0x00 },
                { 0x0FFE, 0x00 },
                { 0x0FFF, 0x00 },
                { 0x1000, 0x55 }, // <- SP
                { 0x1001, 0x33 },
                { 0x1002, 0x00 },
                { 0x1003, 0x00 },
                { 0x1004, 0x00 },
                { 0x1005, 0x00 },
                { 0x1006, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x1000, IY = 0x0000, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x1002, cpu.SP);
            Assert.Equal(0x3355, cpu.IY);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void PushThenPop()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xC5 }, // PUSH BC
                { 0x0081, 0xC1 }, // POP BC
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Stack
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 }, // <-- SP
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x00, SP = 0x0905, B = 0x10, C = 0x11, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Step();

            Assert.Equal(0x0903, cpu.SP);
            Assert.Equal(0x10, program[(ushort)(cpu.SP + 1)]);
            Assert.Equal(0x11, program[(ushort)(cpu.SP)]);

            // Clear BC
            cpu.B = 0x00;
            cpu.C = 0x00;

            cpu.Step();

            Assert.Equal(0x0905, cpu.SP);
            Assert.Equal(0x1011, cpu.BC);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }
    }
}
