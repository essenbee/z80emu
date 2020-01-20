using PixelEngine;
using System;
using System.Diagnostics;
using System.IO;

// **======================================================**
// * This project uses the OLC Pixel Game Engine             *
// *    *Copyright 2018 OneLoneCoder.com*                      *
// *    (https://github.com/OneLoneCoder/olcPixelGameEngine) *
// * C# port of the Pixel Game Engine by DevChrome           *
// *    (https://github.com/DevChrome/Pixel-Engine)          *
// **======================================================**

namespace Essenbee.Z80.Spectrum48
{
    class Program : Game
    {
        private static Z80 _cpu;
        private static string _rom = @"..\..\ROM\48.rom";
        private SimpleBus _simpleBus = null;
        private Point _origin = new Point(47, 47);
        private byte[] _ram;
        private int _renderTicks = 0;

        public Program() => AppName = $"Essenbee.Spectrum48 (OLC Pixel Game Engine)";

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
            _simpleBus = new SimpleBus(_ram);
            _cpu.ConnectToBus(_simpleBus);
            _renderTicks = (int)((1.0f / 4000.0f) * Stopwatch.Frequency); // Tune this value
        }

        public override void OnUpdate(float elapsed)
        {
            Clear(_simpleBus.BorderColour);

            if (Focus)
            {
                if (GetKey(Key.Escape).Pressed)
                {
                    _cpu.Reset();
                }
            }

            while (!_simpleBus.ScreenReady)
            {
                var sw = Stopwatch.StartNew();
                _simpleBus.RunRenderer();

                while (sw.ElapsedTicks < _renderTicks && !_simpleBus.ScreenReady)
                {
                    _cpu.Step();
                }
            }

            _simpleBus.ScreenReady = false;
            AppName = $"Essenbee.Spectrum48 (OLC Pixel Game Engine) rendering @{1.0f / elapsed} FPS";
            DrawSprite(_origin, _simpleBus.GetScreen()); // Get buffered sprite representing the screen
        }

        static void Main(string[] args)
        {
            var spectrum48 = new Program();
            spectrum48.Construct((32 * 8) + 96, (24 * 8) + 104, 2, 2);
            spectrum48.Start();
        }
    }
}
