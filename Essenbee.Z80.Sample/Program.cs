using System;
using System.IO;

namespace Essenbee.Z80.Sample
{
    class Program
    {
        private static Z80 _cpu;
        private static string _rom = @"..\..\..\ROM\48.rom";

        static void Main(string[] args)
        {
            var ram = new byte[65536];
            Array.Clear(ram, 0, ram.Length);
            var romData = File.ReadAllBytes(_rom);

            if (romData.Length != 16384)
            {
                throw new InvalidOperationException("Not a valid ROM file");
            }

            Array.Copy(romData, ram, 16384);
            _cpu = new Z80();
            IBus simpleBus = new SimpleBus(ram);
            _cpu.ConnectToBus(simpleBus);

            Console.Clear();

            while (!(Console.KeyAvailable && (Console.ReadKey(true).Key == ConsoleKey.Escape)))
            {
                try
                {
                    _cpu.Step();
                    Console.Write($"\rPC: {_cpu.PC.ToString("X4")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine();

            for (var i = 0x4000; i < 0x5800; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                {
                    Console.Write("{0:x2} ", ram[i]);
                }

                if (i % 8 == 7)
                {
                    Console.Write("  ");
                }

                if (i % 16 == 15)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
