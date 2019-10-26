using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class EightBitLoadGroupShould
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
        public void LoadAfromBwhenOperationIsLDBA()
        {
            var fakeBus = A.Fake<IBus>();

            // Reading RAM will fetch opcode 0x47, which is LD B,A
            A.CallTo(() => fakeBus.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x47);

            var cpu = new Z80() { A = 0x0F, B = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, cpu.B);
            Assert.Equal(cpu.A, cpu.B);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadBfromAwhenOperationIsLDAB()
        {
            var fakeBus = A.Fake<IBus>();

            // Reading RAM will fetch opcode 0x78, which is LD A,B
            A.CallTo(() => fakeBus.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x78);

            var cpu = new Z80() { A = 0x00, B = 0x0F, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, cpu.A);
            Assert.Equal(cpu.B, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void NoOpwhenOperationIsLDAA()
        {
            var fakeBus = A.Fake<IBus>();

            // Reading RAM will fetch opcode 0x7F, which is LD A,A
            A.CallTo(() => fakeBus.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x7F);

            var cpu = new Z80() { A = 0x03, B = 0x0F, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x03, cpu.A);
            Assert.Equal(0x0F, cpu.B);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueWhenOperationIsLDAn()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new byte[]
            {
                0x3E,
                0x02,
            };

            // Reading RAM will fetch opcode 0x3E, which is LD A,n,
            // and then the operand n (2 in this case)...
            A.CallTo(() => fakeBus.Read(A<ushort>.Ignored, A<bool>.Ignored)).ReturnsNextFromSequence(program);

            // Load 2 into register A, which starts out having a value of 3...
            var cpu = new Z80() { A = 0x03, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x02, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadHwithValueWhenOperationIsLDHn()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new byte[]
            {
                0x26,
                0x1E,
            };

            // Reading RAM will fetch opcode 0x26, which is LD H,n,
            // and then the operand n...
            A.CallTo(() => fakeBus.Read(A<ushort>.Ignored, A<bool>.Ignored)).ReturnsNextFromSequence(program);

            // Load 1E into register A, which starts out having a value of 3...
            var cpu = new Z80() { A = 0x03, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x1E, cpu.H);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinHLwhenOperationIsLDAHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x7E },
                { 0x0081, 0x02 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FF, 0x08 },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
            };

            // Reading RAM will fetch opcode 0x7E, which is LD A, (HL);
            // the operand 0x08 is stored in the location pointed to by HL (0x08FF)...

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, H= 0x08, L = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x08, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIXD_GivenDisZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x7E },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IX
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x08, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIXD_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x7E },
                { 0x0082, 0x03 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IX
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x05, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIXD_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x7E },
                { 0x0082, 0xFF },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IX
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x15, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIYD_GivenDisZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x7E },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IY
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x08, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIYD_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x7E },
                { 0x0082, 0x03 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IY
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x05, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAwithValueAtMemoryLocationinIXpludDwhenOperationIsLDRIYD_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x7E },
                { 0x0082, 0xFF },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x15 },
                { 0x08FF, 0x08 }, // IY
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x05 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x03, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x15, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadMemoryLocationPointedToByHLwithValueInAccumulator()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x77 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (HL)
                { 0x0900, 0x00 },
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

            // Load the value in register A (0x11), into the memory location pointed to by (HL)...
            var cpu = new Z80() { A = 0x11, H = 0x08, L = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x11, program[0x08FF]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIXplusDwithValueInRegister_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x77 },
                { 0x0082, 0x03 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IX)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 }, // <- d = +3
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x0F, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, program[0x08FF + 0x03]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIXplusDwithValueInRegister_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x77 },
                { 0x0082, 0xFE },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 }, // <- d = -2
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IX)
                { 0x0900, 0x00 },
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

            var cpu = new Z80() { A = 0x0F, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, program[0x08FF - 2]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIYplusDwithValueInRegister_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x77 },
                { 0x0082, 0x03 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IY)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 }, // <- d = +3
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x0F, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, program[0x08FF + 0x03]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIYplusDwithValueInRegister_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x77 },
                { 0x0082, 0xFE },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 }, // <- d = -2
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IY)
                { 0x0900, 0x00 },
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

            var cpu = new Z80() { A = 0x0F, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x0F, program[0x08FF - 2]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByHLwithValueN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x36 },
                { 0x0081, 0xA6 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (HL)
                { 0x0900, 0x00 },
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

            var cpu = new Z80() { A = 0x11, H = 0x08, L = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xA6, program[0x08FF]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIXplusDwithValueN_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x36 },
                { 0x0082, 0x03 },
                { 0x0083, 0xAA },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IX)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 }, // <- d = +3
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x0F, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAA, program[0x08FF + 0x03]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIXplusDwithValueN_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x36 },
                { 0x0082, 0xFE },
                { 0x0083, 0xBB },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 }, // <- d = -2
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IX)
                { 0x0900, 0x00 },
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

            var cpu = new Z80() { A = 0x0F, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xBB, program[0x08FF - 2]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIYplusDwithValueN_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x36 },
                { 0x0082, 0x03 },
                { 0x0083, 0xAA },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IY)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 }, // <- d = +3
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);
            A.CallTo(() => fakeBus.Write(A<ushort>._, A<byte>._))
                .Invokes((ushort addr, byte data) => UpdateMemory(addr, data));

            var cpu = new Z80() { A = 0x0F, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xAA, program[0x08FF + 0x03]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByIYplusDwithValueN_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x36 },
                { 0x0082, 0xFE },
                { 0x0083, 0xBB },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 }, // <- d = -2
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (IY)
                { 0x0900, 0x00 },
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

            var cpu = new Z80() { A = 0x0F, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xBB, program[0x08FF - 2]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadAccumulatorWithValueInLocationPointedToByBC()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x0A },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 }, 
                { 0x08FE, 0x00 },
                { 0x08FF, 0x1C }, // (BC)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, B = 0x08, C = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x1C, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAccumulatorWithValueInLocationPointedToByDE()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x1A },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x1C }, // (DE)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, D = 0x08, E = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x1C, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadAccumulatorWithValueInLocationPointedToByNN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x3A },
                { 0x0081, 0xFF },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x1C },
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x00 },
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x1C, cpu.A);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);
        }

        [Fact]
        public void LoadMemoryLocationPointedToByBCwithValueInAccumulator()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x02 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (BC)
                { 0x0900, 0x00 },
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

            // Load the value in register A (0x11), into the memory location pointed to by (BC)...
            var cpu = new Z80() { A = 0x11, B = 0x08, C = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x11, program[0x08FF]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByDEwithValueInAccumulator()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x12 },
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // (DE)
                { 0x0900, 0x00 },
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

            // Load the value in register A (0x11), into the memory location pointed to by (DE)...
            var cpu = new Z80() { A = 0x11, D = 0x08, E = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x11, program[0x08FF]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadMemoryLocationPointedToByNNwithValueInAccumulator()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x32 },
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
                { 0x0900, 0x00 },
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

            // Load the value in register A (0x11), into the memory location pointed to by (DE)...
            var cpu = new Z80() { A = 0x11, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x11, program[0x08FF]);

            // No affect on Condition Flags
            FlagsUnchanged(cpu);

            void UpdateMemory(ushort addr, byte data)
            {
                program[addr] = data;
            }
        }

        [Fact]
        public void LoadAfromIwhenOperationIsLDAI_GivenIisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x57 },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, I = 0x17, IFF2 = true, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x17, cpu.I);
            Assert.Equal(cpu.A, cpu.I);

            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAfromIwhenOperationIsLDAI_GivenIisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x57 },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, I = 0xFA, IFF2 = true, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0xFA, cpu.I);
            Assert.Equal(cpu.A, cpu.I);

            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.True((cpu.F & Z80.Flags.S) == Z80.Flags.S);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAfromIwhenOperationIsLDAI_GivenIisZero()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xED },
                { 0x0081, 0x57 },
                { 0x0082, 0x08 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0F, I = 0x00, IFF2 = true, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x00, cpu.I);
            Assert.Equal(cpu.A, cpu.I);

            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.True((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.True((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }
    }
}
