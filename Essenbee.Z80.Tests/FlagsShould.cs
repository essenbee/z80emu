using System;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class FlagsShould
    {
        [Flags]
        private enum Flags
        {
            C = 1 << 0,
            N = 1 << 1,
            P = 1 << 2,
            X = 1 << 3,
            H = 1 << 4,
            U = 1 << 5,
            Z = 1 << 6,
            S = 1 << 7,
        };

        private Flags _flagRegister;

        [Fact]
        public void InitiallyHaveAllFlagsNotSet()
        {
            _flagRegister = 0x00;
            Assert.False((_flagRegister & Flags.C) == Flags.C);
            Assert.False((_flagRegister & Flags.N) == Flags.N);
            Assert.False((_flagRegister & Flags.P) == Flags.P);
            Assert.False((_flagRegister & Flags.X) == Flags.X);
            Assert.False((_flagRegister & Flags.H) == Flags.H);
            Assert.False((_flagRegister & Flags.U) == Flags.U);
            Assert.False((_flagRegister & Flags.Z) == Flags.Z);
            Assert.False((_flagRegister & Flags.S) == Flags.S);
        }

        [Fact]
        public void SetCarryFlagOnly()
        {
            _flagRegister = 0x00;

            SetFlag(Flags.C, true);

            Assert.True((_flagRegister & Flags.C) == Flags.C);
            Assert.False((_flagRegister & Flags.N) == Flags.N);
            Assert.False((_flagRegister & Flags.P) == Flags.P);
            Assert.False((_flagRegister & Flags.X) == Flags.X);
            Assert.False((_flagRegister & Flags.H) == Flags.H);
            Assert.False((_flagRegister & Flags.U) == Flags.U);
            Assert.False((_flagRegister & Flags.Z) == Flags.Z);
            Assert.False((_flagRegister & Flags.S) == Flags.S);
        }

        [Fact]
        public void SetCarryAndSignFlagOnly()
        {
            _flagRegister = 0x00;

            SetFlag(Flags.C, true);
            SetFlag(Flags.S, true);

            Assert.True((_flagRegister & Flags.C) == Flags.C);
            Assert.False((_flagRegister & Flags.N) == Flags.N);
            Assert.False((_flagRegister & Flags.P) == Flags.P);
            Assert.False((_flagRegister & Flags.X) == Flags.X);
            Assert.False((_flagRegister & Flags.H) == Flags.H);
            Assert.False((_flagRegister & Flags.U) == Flags.U);
            Assert.False((_flagRegister & Flags.Z) == Flags.Z);
            Assert.True((_flagRegister & Flags.S) == Flags.S);
        }

        [Fact]
        public void UnSetZeroFlagOnly()
        {
            _flagRegister = Flags.C | Flags.N | Flags.P | Flags.H | Flags.Z | Flags.S;

            SetFlag(Flags.Z, false);

            Assert.True((_flagRegister & Flags.C) == Flags.C);
            Assert.True((_flagRegister & Flags.N) == Flags.N);
            Assert.True((_flagRegister & Flags.P) == Flags.P);
            Assert.False((_flagRegister & Flags.X) == Flags.X);
            Assert.True((_flagRegister & Flags.H) == Flags.H);
            Assert.False((_flagRegister & Flags.U) == Flags.U);
            Assert.False((_flagRegister & Flags.Z) == Flags.Z);
            Assert.True((_flagRegister & Flags.S) == Flags.S);
        }

        private void SetFlag(Flags flag, bool value)
        {
            if (value)
            {
                _flagRegister |= flag;
            }
            else
            {
                _flagRegister &= ~flag;
            }
        }
    }
}
