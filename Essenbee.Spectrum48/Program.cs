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
        private int[] _keyLine = new int[8];

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
            _cpu = new Z80()
            {
                Z80ClockSpeed = 3_500_000.0f, /* 3.5 MHz*/
            };
            _simpleBus = new SimpleBus(_ram);
            _cpu.ConnectToBus(_simpleBus);
            _renderTicks = (int)((1.0f / 4000.0f) * Stopwatch.Frequency); // Tune this value, looking for 50 FPS
        }

        public override void OnUpdate(float elapsed)
        {
            Clear(_simpleBus.BorderColour);
            var keyMatrix = new int[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, };

            if (Focus)
            {
                if (GetKey(Key.Escape).Pressed)
                {
                    _cpu.Reset();
                }

                keyMatrix = UpdateInput();
            }

            _simpleBus.KeyMatrix = keyMatrix;

            while (!_simpleBus.ScreenReady)
            {
                var sw = Stopwatch.StartNew();
                _simpleBus.RunRenderer();

                while (sw.ElapsedTicks < _renderTicks && !_simpleBus.ScreenReady)
                {
                    _cpu.Step();
                    _simpleBus.Interrupt = false;
                    // Console.WriteLine($"\rPC: {_cpu.PC.ToString("X4")}");
                }
            }

            _simpleBus.ScreenReady = false;
            AppName = $"Essenbee.Spectrum48 (OLC Pixel Game Engine) rendering @{Math.Round(1.0f / elapsed, 1)} FPS";
            DrawSprite(_origin, _simpleBus.GetScreen()); // Get buffered sprite representing the screen
        }

        static void Main(string[] args)
        {
            var spectrum48 = new Program();
            spectrum48.Construct((32 * 8) + 96, (24 * 8) + 104, 3, 3);
            spectrum48.Start();
        }

        public int[] UpdateInput()
        { 
            if (GetKey(Key.Shift).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x1);
            }

            if (GetKey(Key.Z).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x02);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x02);
            }

            if (GetKey(Key.X).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x04);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x04);
            }

            if (GetKey(Key.C).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x08);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x08);
            }

            if (GetKey(Key.V).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x10);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x10);
            }

            if (GetKey(Key.A).Pressed)
            {
                _keyLine[1] = _keyLine[1] & ~(0x1);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x1);
            }

            if (GetKey(Key.S).Pressed)
            {
                _keyLine[1] = _keyLine[1] & ~(0x02);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x02);
            }

            if (GetKey(Key.D).Pressed)
            {
                _keyLine[1] = _keyLine[1] & ~(0x04);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x04);
            }

            if (GetKey(Key.F).Pressed)
            {
                _keyLine[1] = _keyLine[1] & ~(0x08);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x08);
            }

            if (GetKey(Key.G).Pressed)
            {
                _keyLine[1] = _keyLine[1] & ~(0x10);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x10);
            }

            if (GetKey(Key.Q).Pressed)
            {
                _keyLine[2] = _keyLine[2] & ~(0x1);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x1);
            }

            if (GetKey(Key.W).Pressed)
            {
                _keyLine[2] = _keyLine[2] & ~(0x02);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x02);
            }

            if (GetKey(Key.E).Pressed)
            {
                _keyLine[2] = _keyLine[2] & ~(0x04);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x04);
            }

            if (GetKey(Key.R).Pressed)
            {
                _keyLine[2] = _keyLine[2] & ~(0x08);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x08);
            }

            if (GetKey(Key.T).Pressed)
            {
                _keyLine[2] = _keyLine[2] & ~(0x10);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x10);
            }

            if (GetKey(Key.K1).Pressed)
            {
                _keyLine[3] = _keyLine[3] & ~(0x1);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x1);
            }

            if (GetKey(Key.K2).Pressed)
            {
                _keyLine[3] = _keyLine[3] & ~(0x02);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x02);
            }

            if (GetKey(Key.K3).Pressed)
            {
                _keyLine[3] = _keyLine[3] & ~(0x04);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x04);
            }

            if (GetKey(Key.K4).Pressed)
            {
                _keyLine[3] = _keyLine[3] & ~(0x08);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x08);
            }

            if (GetKey(Key.K5).Pressed)
            {
                _keyLine[3] = _keyLine[3] & ~(0x10);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x10);
            }

            if (GetKey(Key.K0).Pressed)
            {
                _keyLine[4] = _keyLine[4] & ~(0x1);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x1);
            }

            if (GetKey(Key.K9).Pressed)
            {
                _keyLine[4] = _keyLine[4] & ~(0x02);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x02);
            }

            if (GetKey(Key.K8).Pressed)
            {
                _keyLine[4] = _keyLine[4] & ~(0x04);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x04);
            }

            if (GetKey(Key.K7).Pressed)
            {
                _keyLine[4] = _keyLine[4] & ~(0x08);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x08);
            }

            if (GetKey(Key.K6).Pressed)
            {
                _keyLine[4] = _keyLine[4] & ~(0x10);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x10);
            }

            if (GetKey(Key.P).Pressed)
            {
                _keyLine[5] = _keyLine[5] & ~(0x1);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x1);
            }

            if (GetKey(Key.O).Pressed)
            {
                _keyLine[5] = _keyLine[5] & ~(0x02);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x02);
            }

            if (GetKey(Key.I).Pressed)
            {
                _keyLine[5] = _keyLine[5] & ~(0x04);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x04);
            }

            if (GetKey(Key.U).Pressed)
            {
                _keyLine[5] = _keyLine[5] & ~(0x08);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x08);
            }

            if (GetKey(Key.Y).Pressed)
            {
                _keyLine[5] = _keyLine[5] & ~(0x10);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x10);
            }

            if (GetKey(Key.Enter).Pressed)
            {
                _keyLine[6] = _keyLine[6] & ~(0x1);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x1);
            }

            if (GetKey(Key.L).Pressed)
            {
                _keyLine[6] = _keyLine[6] & ~(0x02);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x02);
            }

            if (GetKey(Key.K).Pressed)
            {
                _keyLine[6] = _keyLine[6] & ~(0x04);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x04);
            }

            if (GetKey(Key.J).Pressed)
            {
                _keyLine[6] = _keyLine[6] & ~(0x08);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x08);
            }

            if (GetKey(Key.H).Pressed)
            {
                _keyLine[6] = _keyLine[6] & ~(0x10);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x10);
            }

            if (GetKey(Key.Space).Pressed)
            {
                _keyLine[7] = _keyLine[7] & ~(0x1);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x1);
            }

            if (GetKey(Key.Delete).Pressed)
            {
                _keyLine[7] = _keyLine[7] & ~(0x02);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x02);
            }

            if (GetKey(Key.M).Pressed)
            {
                _keyLine[7] = _keyLine[7] & ~(0x04);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x04);
            }

            if (GetKey(Key.N).Pressed)
            {
                _keyLine[7] = _keyLine[7] & ~(0x08);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x08);
            }

            if (GetKey(Key.B).Pressed)
            {
                _keyLine[7] = _keyLine[7] & ~(0x10);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x010);
            }

            return _keyLine;
        }
    }
}
