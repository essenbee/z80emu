using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Essenbee.Z80.Tests.Classes
{
    public class FuseTester
    {
        private const string _testsFile = @"..\..\..\FuseTests\FUSE_Tests.txt";
        private const string _expectedFile = @"..\..\..\FuseTests\FUSE_Expected.txt";
        private Dictionary<string, FuseTest> _tests = new Dictionary<string, FuseTest>();
        private Dictionary<string, FuseExpected> _expected = new Dictionary<string, FuseExpected>();
        class FuseTest
        {
            public List<int> Registers { get; set; } = new List<int>();
            public List<int> States { get; set; } = new List<int>();
            public List<List<int>> Memory { get; set; } = new List<List<int>>();
        }

        class FuseExpected
        {
            public List<List<string>> Events { get; set; } = new List<List<string>>();
            public List<int> Registers { get; set; } = new List<int>();
            public List<int> States { get; set; } = new List<int>();
            public List<List<int>> Memory { get; set; } = new List<List<int>>();
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

                if (line.StartsWith("-1"))
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
                    var regs = line.Split(' ').ToList();
                    regs.RemoveAll(x => x.Equals(string.Empty));
                    var regsHex = regs.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    
                    aTest.Registers = regsHex;
                    lineType++;
                    continue;
                }

                if (lineType == 3)
                {
                    // State data
                    var states = line.Split(' ').ToList();
                    states.RemoveAll(x => x.Equals(string.Empty));
                    var statesHex = states.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    aTest.States = statesHex;
                    lineType++;
                    continue;
                }

                if (lineType == 4)
                {
                    // Memory (1 or more records of this type)
                    var mem = line.Split(' ').ToList();
                    mem.RemoveAll(x => x.Equals(string.Empty));
                    mem.RemoveAll(x => x.Equals("-1"));
                    var memHex = mem.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    aTest.Memory.Add(memHex);
                    continue;
                }
            }
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
                    if (line.StartsWith(" "))
                    {
                        // Event
                        var anEvent = line.Split(' ').ToList();
                        anEvent.RemoveAll(x => x.Equals(string.Empty));

                        anExpected.Events.Add(anEvent);
                        continue;
                    }

                    lineType++;
                }

                if (lineType == 3)
                {
                    // Registers
                    var regs = line.Split(' ').ToList();
                    regs.RemoveAll(x => x.Equals(string.Empty));
                    var regsHex = regs.Select(hex => Convert.ToInt32(hex, 16)).ToList();

                    anExpected.Registers = regsHex;
                    lineType++;
                    continue;
                }

                if (lineType == 4)
                {
                    // State data
                    var states = line.Split(' ').ToList();
                    states.RemoveAll(x => x.Equals(string.Empty));
                    var statesHex = states.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    anExpected.States = statesHex;
                    lineType++;
                    continue;
                }

                if (lineType == 5)
                {
                    // Memory (1 or more records of this type)
                    var mem = line.Split(' ').ToList();
                    mem.RemoveAll(x => x.Equals(string.Empty));
                    mem.RemoveAll(x => x.Equals("-1"));
                    var memHex = mem.Select(hex => Convert.ToInt32(hex, 16)).ToList();
                    anExpected.Memory.Add(memHex);
                    continue;
                }
            }
        }
    }
}
