using FakeItEasy;
using System.Collections.Generic;
using Xunit;

namespace Essenbee.Z80.Tests
{
    public class IsSupportedShould
    {
        [Fact]
        private void ReturnTrueForNOP()
        {
            var cpu = new Z80();

            var isSupported = cpu.IsOpCodeSupported("00");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnTrueForDD09()
        {
            var cpu = new Z80();

            var isSupported = cpu.IsOpCodeSupported("dd09");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnTrueForDD70()
        {
            var cpu = new Z80();

            var isSupported = cpu.IsOpCodeSupported("DD70");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnTrueForDDCB06()
        {
            var cpu = new Z80();

            // DDCB instructions are in the format DDCB{displacement}{opcode}
            var isSupported = cpu.IsOpCodeSupported("DDCB0206");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnTrueForFDCBO206()
        {
            var cpu = new Z80();

            // FDCB instructions are in the format FDCB{displacement}{opcode}
            var isSupported = cpu.IsOpCodeSupported("FDCB0206");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnTrueForFDCB06()
        {
            var cpu = new Z80();

            // FDCB instructions are in the format FDCB{opcode} in FUSE tester
            var isSupported = cpu.IsOpCodeSupported("fdcb06");

            Assert.True(isSupported);
        }

        [Fact]
        private void ReturnFalseForDD00()
        {
            var cpu = new Z80();

            var isSupported = cpu.IsOpCodeSupported("DD00");

            Assert.False(isSupported);
        }

        [Fact]
        private void ReturnFalseForED()
        {
            var cpu = new Z80();

            var isSupported = cpu.IsOpCodeSupported("ED");

            Assert.False(isSupported);
        }
    }
}
