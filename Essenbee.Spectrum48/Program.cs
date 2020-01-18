using System;
using System.IO;
using PixelEngine;

namespace Essenbee.Z80.Spectrum48
{
    class Program : Game
    {
        private static Z80 _cpu;
        private static string _rom = @"..\..\ROM\48.rom";
        private float _countdown = 0.0f;
        private bool _runEmulation = false;
        private byte[] _ram;
        private ushort _screen = 0x4000;

        public override void OnCreate()
        {
            _ram = new byte[65536];
            Array.Clear(_ram, 0, _ram.Length);
            var romData = File.ReadAllBytes(_rom);

            if (romData.Length != 16384)
            {
                throw new InvalidOperationException("Not a valid ROM file");
            }

            Array.Copy(romData, _ram, 16384);
            _cpu = new Z80();
            IBus simpleBus = new SimpleBus(_ram);
            _cpu.ConnectToBus(simpleBus);
        }

        public override void OnUpdate(float elapsed)
        {
            Clear(Pixel.Presets.Blue);

            if (GetKey(Key.Space).Pressed)
            {
                _runEmulation = !_runEmulation;
            }

            if (_runEmulation)
            {
                if (_countdown > 0.0f)
                {
                    _countdown -= elapsed;
                }
                else
                {
                    _countdown += (1.0f / 60.0f) - elapsed;

                    for (int i = 0; i < 100; i++)
                    {
                        _cpu.Step();
                        Console.Write($"\rPC: {_cpu.PC.ToString("X4")}");
                    }
                }

                PaintScreen();
            }
        }

        public void PaintScreen()
        {
            if (_ram != null && _ram.Length >= _screen + 0x1800)
            {
                for (var screenThird = 0; screenThird < 3; ++screenThird)
                {
                    for (var charLine = 0; charLine < 8; ++charLine)
                    {
                        for (var line = 0; line < 8; ++line)
                        {
                            var memOffset = _screen + (screenThird << 11) + (charLine << 8) + (line << 5);
                            var y = (screenThird * 8 * 8) + (line * 8) + (charLine);

                            for (var character = 0; character < 32; ++character)
                            {
                                var c = _ram[memOffset + character]; // Get byte value

                                // Decode bits
                                for (var bitPos = 0; bitPos < 8; ++bitPos)
                                {
                                    var b = 0x1 & (c >> (7 - bitPos));

                                    // ToDo: Add support for colours via the attribute area of RAM...
                                    if (b != 0)
                                    {
                                        Draw((character * 8) + bitPos, y, Pixel.Presets.White);
                                    }
                                    else
                                    {
                                        Draw((character * 8) + bitPos, y, Pixel.Presets.Blue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var spectrum48 = new Program();
            spectrum48.Construct(32 * 8, 24 * 8, 4, 4);
            spectrum48.Start();
        }
    }
}
