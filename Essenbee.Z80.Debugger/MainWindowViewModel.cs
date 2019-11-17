using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Essenbee.Z80.Debugger
{
    public partial class MainWindowViewModel
    {
        private Z80 _cpu;
        private IBus _basicBus;

        // ================== Construction Event ==================
        partial void Constructed()
        {
            _basicBus = new BasicBus(64);
            _cpu = new Z80 { PC = 0x8000 }; //Default start location
            _cpu.ConnectToBus(_basicBus);
            ProgramCounter = _cpu.PC.ToString("X4");
            Memory = BuildMemoryMap();
        }

        // ================== Property Events ==================
        partial void Changed_ProgramCounter(string prev, string current)
        {
            _cpu.PC = ushort.Parse(current, System.Globalization.NumberStyles.HexNumber) ;
        }

        // ================== Command Events ==================
        partial void CanExecute_StepCommand(ref bool result)
        {
            result = true;
        }

        partial void Execute_StepCommand()
        {



            Memory = BuildMemoryMap();
        }

        partial void CanExecute_LoadCommand(ref bool result)
        {
            result = true;
        }

        partial void Execute_LoadCommand()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "HEX file (*.hex)|*.hex"
            };

            var result = openFileDialog.ShowDialog();

            if (result ?? false)
            {
                var fileName = openFileDialog.FileName;
                var RAM = HexFileLoader.Read(fileName, new byte[64 * 1024]);
                _basicBus = new BasicBus(RAM);
                Memory = BuildMemoryMap();


                // ToDo: temporarily memory display as a string
                //Memory = new ObservableCollection<string>(_basicBus.RAM.Select(b => b.ToString("X2")));
            }
        }

        private Dictionary<string, string> BuildMemoryMap()
        {
            var memory = _basicBus.RAM.Select((a, b) => new { a, b })
                                  .ToDictionary(mem => mem.b, mem => mem.a);
            var memoryMap = new Dictionary<string, string>();
            for (int i = 0; i < memory.Count; i += 16)
            {
                memoryMap[i.ToString("X4")] = memory[i].ToString("X2") + " ";

                for (int x = 1; x < 15; x++)
                {
                    memoryMap[i.ToString("X4")] += memory[i + x].ToString("X2") + " ";
                }

                memoryMap[i.ToString("X4")] += memory[i + 15].ToString("X2");
            }

            return memoryMap;
        }
    }
}
