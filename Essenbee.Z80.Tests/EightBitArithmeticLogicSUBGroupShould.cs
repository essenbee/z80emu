using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class EightBitArithmeticLogicSUBGroupShould
    {
        private static void CheckFlags(Z80 cpu, int a, int b, int c = 0)
        {
            // Checks values of S, Z, H, P and C flags
            var trueDiff = (a - b - c);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;

            Assert.Equal(byteDiff, cpu.A);
            Assert.Equal(sbyteDiff < 0, (cpu.F & Z80.Flags.S) == Z80.Flags.S);
            Assert.Equal(cpu.A == 0x00, (cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.Equal(((a & 0xF) < (b & 0xF) + c), (cpu.F & Z80.Flags.H) == Z80.Flags.H);

            // If both operands are positive and result is negative 
            // or if both operands are negative and result is positive
            var overflow = (a < 0x7F && b < 0x7F && sbyteDiff < 0) || 
                           (a >= 0x80 && b > 0x80 && sbyteDiff > 0);

            Assert.Equal(overflow, (cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.Equal(trueDiff > 0xFF, (cpu.F & Z80.Flags.C) == Z80.Flags.C);
        }

        [Fact]
        public void LoadAWithThreeWhenSubtracting8And5ForOpcodeSUBAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD6 }, // SUB A, n
                { 0x0081, 0x05 }, // n = 5
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x08, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 8, 5);

            Assert.Equal(0x03, cpu.A);
            sbyte signedResult = (sbyte)cpu.A;
            Assert.Equal(3, signedResult);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithNegative6WhenSubtracting6And12ForOpcodeSUBAN()
        {
            // ======================================
            // Testing a negative result: 6 - 12 = -6
            // ======================================

            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD6 }, // SUB A, n
                { 0x0081, 0x0C }, // n = 12
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x06, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 6, 12);

            Assert.Equal(0xFA, cpu.A); // FA = 1111 10101 = -6 in 2s complement
            sbyte signedResult = (sbyte)cpu.A;
            Assert.Equal(-6, signedResult);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.True((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithZeroWhenSubtractingZeroFromZeroForOpcodeSUBAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD6 }, // SUB A, n
                { 0x0081, 0x00 }, // n = 0
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 0, 0);

            Assert.Equal(0x00, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith129WhenSubtracting2From131ForOpcodeSUBAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD6 }, // SUB A, n
                { 0x0081, 0x02 }, // n = 2
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x83, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 131, 2);

            Assert.Equal(0x81, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithZeroWhenSubtracting129From129ForOpcodeSUBAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xD6 }, // SUB A, n
                { 0x0081, 0x81 }, // n = 129
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x81, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 129, 129);

            Assert.Equal(0x00, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithTwoWhenSubtracting8And5AndCarryBitSetForOpcodeSBCAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDE }, // SBC A, n
                { 0x0081, 0x05 }, // n = 5
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x08, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 8, 5, 1);

            Assert.Equal(0x02, cpu.A);
            sbyte signedResult = (sbyte)cpu.A;
            Assert.Equal(2, signedResult);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithNegative7WhenSubtracting6And12AndCarryFlagSetForOpcodeSBCAN()
        {
            // ======================================
            // Testing a negative result: 6 - 12 - 1 = -6
            // ======================================

            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDE }, // SBC A, n
                { 0x0081, 0x0C }, // n = 12
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x06, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 6, 12, 1);

            Assert.Equal(0xF9, cpu.A); 
            sbyte signedResult = (sbyte)cpu.A;
            Assert.Equal(-7, signedResult);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.True((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWithZeroWhenSubtractingOneFromTwoAndCarryFlagSetForOpcodeSBCAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDE }, // SUB A, n
                { 0x0081, 0x01 }, // n = 1
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x02, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 2, 1, 1);

            Assert.Equal(0x00, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith128WhenSubtracting2From131AndCarryFlagSetForOpcodeSBCAN()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDE }, // SUB A, n
                { 0x0081, 0x02 }, // n = 2
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x83, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 131, 2, 1);

            Assert.Equal(0x80, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N); // Set due to a subtraction
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith8WhenSubtracting12And4FromBForOpcodeSUBAR()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x90 }, // SUB A, B
                { 0x0081, 0x00 }, 
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0C, B = 0x04, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 12, 4);

            Assert.Equal(0x08, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith7WhenSubtracting4FromBAndCarryFlagSetForOpcodeSBCAR()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x98 }, //SBC A, B
                { 0x0081, 0x00 }, 
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
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
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0C, B = 0x04, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 12, 4, 1);

            Assert.Equal(0x07, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith10WhenSubtracting4FromLocationPointedToByHLForOpcodeSUBAHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x96 }, // SUB A, (HL)
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x04 }, // <- (HL)
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

            var cpu = new Z80() { A = 0x0E, H = 0x08, L = 0xFF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

         CheckFlags(cpu, 14, 4);

            Assert.Equal(0x0A, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith9WhenSubtracting4AndLocationPointedToByHLAndCarryFlagSetForOpcodeSBCAHL()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0x9E }, // SBC A, (HL)
                { 0x0081, 0x00 },
                { 0x0082, 0x00 },
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x04 }, // <- (HL)
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

            var cpu = new Z80() { A = 0x0E, H = 0x08, L = 0xFF, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            Assert.Equal(0x09, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith6WhenSubtractingLocationPointedToByIXFrom10ForOpcodeSUBAIXD_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x96 }, // SUB A, (IX+d)
                { 0x0082, 0xFE }, // d = -2
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x04 }, // (IX-2)
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // <- (IX)
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

            var cpu = new Z80() { A = 0x0A, IX = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 10, 4);

            Assert.Equal(0x06, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith10WhenSubtracting4FromLocationPointedToByIYForOpcodeSUBAIYD_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x96 }, // SUB A, (IY+d)
                { 0x0082, 0x03 }, // d = 3
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // <- (IY)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x04 }, // (IY+3)
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0E, IY = 0x08FF, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 14, 4);

            Assert.Equal(0x0A, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith5WhenSubtractingLocationPointedToByIXFrom10ForOpcodeSBCAIXD_GivenDisNegative()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xDD },
                { 0x0081, 0x9E }, // SBC A, (IX+d)
                { 0x0082, 0xFE }, // d = -2
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x04 }, // (IX-2)
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // <- (IX)
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

            var cpu = new Z80() { A = 0x0A, IX = 0x08FF, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 10, 4, 1);

            Assert.Equal(0x05, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }

        [Fact]
        public void LoadAWith9WhenSubtracting4FromLocationPointedToByIYForOpcodeSBCAHL_GivenDisPositive()
        {
            var fakeBus = A.Fake<IBus>();

            var program = new Dictionary<ushort, byte>
            {
                // Program Code
                { 0x0080, 0xFD },
                { 0x0081, 0x9E }, // SBC A, (IY+d)
                { 0x0082, 0x03 }, // d = 3
                { 0x0083, 0x00 },
                { 0x0084, 0x00 },

                // Data
                { 0x08FB, 0x00 },
                { 0x08FC, 0x00 },
                { 0x08FD, 0x00 },
                { 0x08FE, 0x00 },
                { 0x08FF, 0x00 }, // <- (IY)
                { 0x0900, 0x00 },
                { 0x0901, 0x00 },
                { 0x0902, 0x04 }, // (IY+3)
                { 0x0903, 0x00 },
                { 0x0904, 0x00 },
                { 0x0905, 0x00 },
                { 0x0906, 0x00 },
            };

            A.CallTo(() => fakeBus.Read(A<ushort>._, A<bool>._))
                .ReturnsLazily((ushort addr, bool ro) => program[addr]);

            var cpu = new Z80() { A = 0x0E, IY = 0x08FF, F = Z80.Flags.C, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus);
            cpu.Tick();

            CheckFlags(cpu, 14, 4, 1);

            Assert.Equal(0x09, cpu.A);
            Assert.True((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.True((cpu.F & Z80.Flags.X) == Z80.Flags.X);
        }
    }
}
