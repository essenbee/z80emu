using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class EightBitLoadGroupShould
    {
        [Fact]
        public void LoadAfromBwhenOperationIsLDBA()
        {
            var fakeBus = new Fake<IBus>();

            // Reading RAM will fetch opcode 0x47, which is LD B,A
            A.CallTo(() => fakeBus.FakedObject.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x47);

            var cpu = new Z80() { A = 0x0F, B = 0x00, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus.FakedObject);
            cpu.Tick();

            Assert.Equal(0x0F, cpu.B);
            Assert.Equal(cpu.A, cpu.B);

            // No affect on Condition Flags
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
        public void LoadBfromAwhenOperationIsLDAB()
        {
            var fakeBus = new Fake<IBus>();

            // Reading RAM will fetch opcode 0x78, which is LD A,B
            A.CallTo(() => fakeBus.FakedObject.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x78);

            var cpu = new Z80() { A = 0x00, B = 0x0F, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus.FakedObject);
            cpu.Tick();

            Assert.Equal(0x0F, cpu.A);
            Assert.Equal(cpu.B, cpu.A);

            // No affect on Condition Flags
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
        public void NoOpwhenOperationIsLDAA()
        {
            var fakeBus = new Fake<IBus>();

            // Reading RAM will fetch opcode 0x7F, which is LD A,A
            A.CallTo(() => fakeBus.FakedObject.Read(A<ushort>.Ignored, A<bool>.Ignored)).Returns<byte>(0x7F);

            var cpu = new Z80() { A = 0x00, B = 0x0F, PC = 0x0080 };
            cpu.ConnectToBus(fakeBus.FakedObject);
            cpu.Tick();

            Assert.Equal(0x00, cpu.A);
            Assert.Equal(0x0F, cpu.B);

            // No affect on Condition Flags
            Assert.False((cpu.F & Z80.Flags.C) == Z80.Flags.C);
            Assert.False((cpu.F & Z80.Flags.N) == Z80.Flags.N);
            Assert.False((cpu.F & Z80.Flags.P) == Z80.Flags.P);
            Assert.False((cpu.F & Z80.Flags.X) == Z80.Flags.X);
            Assert.False((cpu.F & Z80.Flags.H) == Z80.Flags.H);
            Assert.False((cpu.F & Z80.Flags.U) == Z80.Flags.U);
            Assert.False((cpu.F & Z80.Flags.Z) == Z80.Flags.Z);
            Assert.False((cpu.F & Z80.Flags.S) == Z80.Flags.S);
        }
    }
}
