using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Essenbee.Z80.Z80;

namespace Essenbee.Z80.Tests.Classes
{
    public class FuseTester
    {
        private const string _testsFile = @"..\..\..\FuseTests\FUSE_Tests.txt";
        private const string _expectedFile = @"..\..\..\FuseTests\FUSE_Expected.txt";
        private Dictionary<string, FuseTest> _tests = new Dictionary<string, FuseTest>();
        private Dictionary<string, FuseExpected> _expected = new Dictionary<string, FuseExpected>();
        private Z80 _cpu;

        private BasicBus _bus;

        private List<string> _passing = new List<string>();
        private Dictionary<string, List<string>> _failing = new Dictionary<string, List<string>>();
        private List<string> _notImplemented = new List<string>();

        public class Results
        {
            public List<string> Passing = new List<string>();
            public Dictionary<string, List<string>> Failing = new Dictionary<string, List<string>>();
            public List<string> NotImplemented = new List<string>();
        }

        class FuseTest
        {
            public List<ushort> Registers { get; set; } = new List<ushort>();
            public List<int> States { get; set; } = new List<int>();
            public List<List<int>> Memory { get; set; } = new List<List<int>>();
        }

        class FuseExpected
        {
            public List<List<string>> Events { get; set; } = new List<List<string>>();
            public List<ushort> Registers { get; set; } = new List<ushort>();
            public List<int> States { get; set; } = new List<int>();
            public List<List<int>> Memory { get; set; } = new List<List<int>>();
        }

        public Results RunTests()
        {
            ReadFuseTestsFile();
            ReadFuseExpectedFile();
            _cpu = new Z80();

            foreach (var test in _tests)
            {
                var testName = test.Key;
                var opCode = testName.Split('_')[0];

                _cpu.Reset(true);

                if (_cpu.IsOpCodeSupported(opCode))
                {
                    // Run test
                    var testToRun = test.Value;
                    var registers = testToRun.Registers;
                    var memory = testToRun.Memory;

                    InitialiseRegisters(registers);
                    InitialiseMemory(memory);
                    _cpu.ConnectToBus(_bus);

                    var runToAddress = _expected[testName].Registers[11];

                    do
                    {
                        _cpu.Step();
                    } while (_cpu.PC < runToAddress);

                    var (pass, details) = CompareActualWithExpected(_expected[testName]);

                    if (pass)
                    {
                        _passing.Add(testName);
                    }
                    else
                    {
                        _failing.Add(testName, details);
                    }
                }
                else
                {
                    _notImplemented.Add(testName);
                }
            }

            return new Results
            {
                Passing = _passing,
                Failing = _failing,
                NotImplemented = _notImplemented,
            };
        }

        private void InitialiseRegisters(List<ushort> registers)
        {
            _cpu.A = (byte)((registers[0] & 0xFF00) >> 8);
            _cpu.F = (Flags)(registers[0] & 0x00FF);
            _cpu.B = (byte)((registers[1] & 0xFF00) >> 8);
            _cpu.C = (byte)(registers[1] & 0x00FF);
            _cpu.D = (byte)((registers[2] & 0xFF00) >> 8);
            _cpu.E = (byte)(registers[2] & 0x00FF);
            _cpu.H = (byte)((registers[3] & 0xFF00) >> 8);
            _cpu.L = (byte)(registers[3] & 0x00FF);
            _cpu.A1 = (byte)((registers[4] & 0xFF00) >> 8);
            _cpu.F1 = (Flags)(registers[4] & 0x00FF);
            _cpu.B1 = (byte)((registers[5] & 0xFF00) >> 8);
            _cpu.C1 = (byte)(registers[5] & 0x00FF);
            _cpu.D1 = (byte)((registers[6] & 0xFF00) >> 8);
            _cpu.E1 = (byte)(registers[6] & 0x00FF);
            _cpu.H1 = (byte)((registers[7] & 0xFF00) >> 8);
            _cpu.L1 = (byte)(registers[7] & 0x00FF);

            _cpu.IX = registers[8];
            _cpu.IY = registers[9];
            _cpu.SP = registers[10];
            _cpu.PC = registers[11];
        }
        private void InitialiseMemory(List<List<int>> memory)
        {
            _bus = new BasicBus(64);

            foreach (var memBlock in memory)
            {
                var address = (ushort)memBlock[0];
                var data = memBlock.ToArray()[1..];

                foreach (var item in data)
                {
                    _bus.Write(address, (byte)item);
                    address++;
                }
            }
        }

        public void ReadFuseTestsFile()
        {
            var lines = File.ReadAllLines(_testsFile);
            var aTest = new FuseTest(); ;
            var newTest = true;
            var lineType = 1;
            var testName = string.Empty;

            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                if (newTest)
                {
                    aTest = new FuseTest();
                    newTest = false;
                }

                if (line.StartsWith("-1", StringComparison.InvariantCultureIgnoreCase))
                {
                    _tests.Add(testName, aTest);
                    newTest = true;
                    lineType = 1;
                    continue;
                }

                if (lineType == 1)
                {
                    // Test name
                    testName = line;
                    lineType++;
                    continue;
                }

                if (lineType == 2)
                {
                    // Registers
                    // AF BC DE HL AF' BC' DE' HL' IX IY SP PC MEMPTR
                    var regs = line.Split(' ').ToList();
                    regs.RemoveAll(x => string.IsNullOrEmpty(x));
                    var regsHex = regs.Select(hex => (ushort)Convert.ToInt32(hex, 16)).ToList();
                    
                    aTest.Registers = regsHex;
                    lineType++;
                    continue;
                }

                if (lineType == 3)
                {
                    // State data
                    // I R IFF1 IFF2 IM <halted> <t-states>
                    var states = line.Split(' ').ToList();
                    states.RemoveAll(x => string.IsNullOrEmpty(x));
                    var statesHex = states.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    aTest.States = statesHex;
                    lineType++;
                    continue;
                }

                if (lineType == 4)
                {
                    // Memory (1 or more records of this type)
                    // <start address> <byte1> <byte2> ... -1
                    var mem = line.Split(' ').ToList();
                    mem.RemoveAll(x => string.IsNullOrEmpty(x));
                    mem.RemoveAll(x => x.Equals("-1", StringComparison.InvariantCultureIgnoreCase));
                    var memHex = mem.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    aTest.Memory.Add(memHex);
                    continue;
                }
            }
        }
        private (bool, List<string>) CompareActualWithExpected(FuseExpected expected)
        {
            var retVal = true;
            var details = new List<string>();
            var expectedRegisters = expected.Registers;
            var expectedMemory = expected.Memory;

            // Registers
            var afCompare = _cpu.AF == expectedRegisters[0];
            var bcCompare = _cpu.BC == expectedRegisters[1];
            var deCompare = _cpu.DE == expectedRegisters[2];
            var hlCompare = _cpu.HL == expectedRegisters[3];
            var af1Compare = _cpu.AF1 == expectedRegisters[4];
            var bc1Compare = _cpu.BC1 == expectedRegisters[5];
            var de1Compare = _cpu.DE1 == expectedRegisters[6];
            var hl1Compare = _cpu.HL1 == expectedRegisters[7];
            var ixCompare = _cpu.IX == expectedRegisters[8];
            var iyCompare = _cpu.IY == expectedRegisters[9];
            var spCompare = _cpu.SP == expectedRegisters[10];
            var pcCompare = _cpu.PC == expectedRegisters[11];
            var ptrCompare = _cpu.MEMPTR == expectedRegisters[12];

            if (!afCompare)
            {
                details.Add($"AF expected {expectedRegisters[0]} got {_cpu.AF}");
                retVal = false;
            }
            if (!bcCompare)
            {
                details.Add($"BC expected {expectedRegisters[1]} got {_cpu.BC}");
                retVal = false;
            }

            if (!deCompare)
            {
                details.Add($"DE expected {expectedRegisters[2]} got {_cpu.DE}");
                retVal = false;
            }

            if (!hlCompare)
            {
                details.Add($"HL expected {expectedRegisters[3]} got {_cpu.HL}");
                retVal = false;
            }

            if (!af1Compare)
            {
                details.Add($"AF' expected {expectedRegisters[4]} got {_cpu.AF1}");
                retVal = false;
            }

            if (!bc1Compare)
            {
                details.Add($"BC1 expected {expectedRegisters[5]} got {_cpu.BC1}");
                retVal = false;
            }

            if (!de1Compare)
            {
                details.Add($"DE1 expected {expectedRegisters[6]} got {_cpu.DE1}");
                retVal = false;
            }

            if (!hl1Compare)
            {
                details.Add($"HL1 expected {expectedRegisters[7]} got {_cpu.HL1}");
                retVal = false;
            }

            if (!ixCompare)
            {
                details.Add($"IX expected {expectedRegisters[8]} got {_cpu.IX}");
                retVal = false;
            }

            if (!iyCompare)
            {
                details.Add($"IY expected {expectedRegisters[9]} got {_cpu.IY}");
                retVal = false;
            }

            if (!spCompare)
            {
                details.Add($"SP expected {expectedRegisters[10]} got {_cpu.SP}");
                retVal = false;
            }

            if (!pcCompare)
            {
                details.Add($"PC expected {expectedRegisters[11]} got {_cpu.PC}");
                retVal = false;
            }

            //if (!ptrCompare)
            //{
            //    details.Add($"MEMPTR expected {expectedRegisters[12]} got {_cpu.MEMPTR}");
            //    retVal = false;
            //}

            return (retVal, details);
        }

        public void ReadFuseExpectedFile()
        {
            var lines = File.ReadAllLines(_expectedFile);
            var anExpected = new FuseExpected(); ;
            var newExptected = true;
            var lineType = 1;
            var testName = string.Empty;

            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    _expected.Add(testName, anExpected);
                    newExptected = true;
                    lineType = 1;
                    continue;
                }

                if (newExptected)
                {
                    anExpected = new FuseExpected();
                    newExptected = false;
                }


                if (lineType == 1)
                {
                    // Test name
                    testName = line;
                    lineType++;
                    continue;
                }

                if (lineType == 2)
                {
                    if (line.StartsWith(" ", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Event - not used by my tests
                        // <time> <type> <address> <data>
                        var anEvent = line.Split(' ').ToList();
                        anEvent.RemoveAll(x => string.IsNullOrEmpty(x));

                        anExpected.Events.Add(anEvent);
                        continue;
                    }

                    lineType++;
                }

                if (lineType == 3)
                {
                    // Registers
                    // AF BC DE HL AF' BC' DE' HL' IX IY SP PC MEMPTR
                    var regs = line.Split(' ').ToList();
                    regs.RemoveAll(x => string.IsNullOrEmpty(x));
                    var regsHex = regs.Select(hex => (ushort)Convert.ToInt32(hex, 16)).ToList();

                    anExpected.Registers = regsHex;
                    lineType++;
                    continue;
                }

                if (lineType == 4)
                {
                    // State data
                    // I R IFF1 IFF2 IM <halted> <t-states>
                    var states = line.Split(' ').ToList();
                    states.RemoveAll(x => string.IsNullOrEmpty(x));
                    var statesHex = states.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    anExpected.States = statesHex;
                    lineType++;
                    continue;
                }

                if (lineType == 5)
                {
                    // Memory (1 or more records of this type)
                    // <start address> <byte1> <byte2> ... -1
                    var mem = line.Split(' ').ToList();
                    mem.RemoveAll(x => string.IsNullOrEmpty(x));
                    mem.RemoveAll(x => x.Equals("-1", StringComparison.InvariantCultureIgnoreCase));
                    var memHex = mem.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    anExpected.Memory.Add(memHex);
                    continue;
                }
            }
        }
    }
}
