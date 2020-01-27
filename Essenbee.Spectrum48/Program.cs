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
    //`
    //` ![](CF3C03E9AEA956395291BE98C580C27F.png)
    //`
    //` 
    //` http://www.worldofspectrum.org/ZXBasicManual/zxmanchap1.html

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

            // I'm guessing: run CPU steps for about 80% of the time to render 1/64th of a frame @ ~50fps
            // The figures may need tuning for the PC you are running this code on...
            _renderTicks = (int)(0.8f * (0.02f / 64.0f) * Stopwatch.Frequency);
        }

        public override void OnUpdate(float elapsed)
        {
            Clear(_simpleBus.BorderColour);
            var keyMatrix = new int[8];

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

            if (Focus)
            {
                if (GetKey(Key.Escape).Pressed)
                {
                    _cpu.Reset();
                }

                if (GetKey(Key.F1).Pressed)
                {
                    try
                    {
                        var snapshot = Z80FileReader.LoadZ80File("D:/Snapshots/Dizzy2.z80");

                        if (snapshot.Type == 0)
                        {
                            SetUpMachineState(snapshot);
                        }
                        else
                        {
                            Console.WriteLine("** Not a Spectrum 48K file! **");
                        }
                    }
                    catch
                    {
                        Console.WriteLine("** Error loading .z80 file! **");
                    }
                }

                keyMatrix = UpdateInput();
            }

            _simpleBus.ScreenReady = false;
            _simpleBus.KeyMatrix = keyMatrix;
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
            // Caps Shift
            if (GetKey(Key.Shift).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x1);
            }

            if (GetKey(Key.Z).Pressed || GetKey(Key.Z).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x02);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x02);
            }

            if (GetKey(Key.X).Pressed || GetKey(Key.X).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x04);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x04);
            }

            if (GetKey(Key.C).Pressed || GetKey(Key.C).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x08);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x08);
            }

            if (GetKey(Key.V).Pressed || GetKey(Key.V).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x10);
            }
            else
            {
                _keyLine[0] = _keyLine[0] | (0x10);
            }

            if (GetKey(Key.A).Pressed || GetKey(Key.A).Down)
            {
                _keyLine[1] = _keyLine[1] & ~(0x1);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x1);
            }

            if (GetKey(Key.S).Pressed || GetKey(Key.S).Down)
            {
                _keyLine[1] = _keyLine[1] & ~(0x02);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x02);
            }

            if (GetKey(Key.D).Pressed || GetKey(Key.D).Down)
            {
                _keyLine[1] = _keyLine[1] & ~(0x04);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x04);
            }

            if (GetKey(Key.F).Pressed || GetKey(Key.F).Down)
            {
                _keyLine[1] = _keyLine[1] & ~(0x08);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x08);
            }

            if (GetKey(Key.G).Pressed || GetKey(Key.G).Down)
            {
                _keyLine[1] = _keyLine[1] & ~(0x10);
            }
            else
            {
                _keyLine[1] = _keyLine[1] | (0x10);
            }

            if (GetKey(Key.Q).Pressed || GetKey(Key.Q).Down)
            {
                _keyLine[2] = _keyLine[2] & ~(0x1);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x1);
            }

            if (GetKey(Key.W).Pressed || GetKey(Key.W).Down)
            {
                _keyLine[2] = _keyLine[2] & ~(0x02);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x02);
            }

            if (GetKey(Key.E).Pressed || GetKey(Key.E).Down)
            {
                _keyLine[2] = _keyLine[2] & ~(0x04);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x04);
            }

            if (GetKey(Key.R).Pressed || GetKey(Key.R).Down)
            {
                _keyLine[2] = _keyLine[2] & ~(0x08);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x08);
            }

            if (GetKey(Key.T).Pressed || GetKey(Key.T).Down)
            {
                _keyLine[2] = _keyLine[2] & ~(0x10);
            }
            else
            {
                _keyLine[2] = _keyLine[2] | (0x10);
            }

            if (GetKey(Key.K1).Pressed || GetKey(Key.K1).Down)
            {
                _keyLine[3] = _keyLine[3] & ~(0x1);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x1);
            }

            if (GetKey(Key.K2).Pressed || GetKey(Key.K2).Down)
            {
                _keyLine[3] = _keyLine[3] & ~(0x02);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x02);
            }

            if (GetKey(Key.K3).Pressed || GetKey(Key.K3).Down)
            {
                _keyLine[3] = _keyLine[3] & ~(0x04);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x04);
            }

            if (GetKey(Key.K4).Pressed || GetKey(Key.K4).Down)
            {
                _keyLine[3] = _keyLine[3] & ~(0x08);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x08);
            }

            if (GetKey(Key.K5).Pressed || GetKey(Key.K5).Down)
            {
                _keyLine[3] = _keyLine[3] & ~(0x10);
            }
            else
            {
                _keyLine[3] = _keyLine[3] | (0x10);
            }

            if (GetKey(Key.K0).Pressed || GetKey(Key.K0).Down)
            {
                _keyLine[4] = _keyLine[4] & ~(0x1);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x1);
            }

            if (GetKey(Key.K9).Pressed || GetKey(Key.K9).Down)
            {
                _keyLine[4] = _keyLine[4] & ~(0x02);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x02);
            }

            if (GetKey(Key.K8).Pressed || GetKey(Key.K8).Down)
            {
                _keyLine[4] = _keyLine[4] & ~(0x04);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x04);
            }

            if (GetKey(Key.K7).Pressed || GetKey(Key.K7).Down)
            {
                _keyLine[4] = _keyLine[4] & ~(0x08);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x08);
            }

            if (GetKey(Key.K6).Pressed || GetKey(Key.K6).Down)
            {
                _keyLine[4] = _keyLine[4] & ~(0x10);
            }
            else
            {
                _keyLine[4] = _keyLine[4] | (0x10);
            }

            if (GetKey(Key.P).Pressed || GetKey(Key.P).Down)
            {
                _keyLine[5] = _keyLine[5] & ~(0x1);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x1);
            }

            if (GetKey(Key.O).Pressed || GetKey(Key.O).Down)
            {
                _keyLine[5] = _keyLine[5] & ~(0x02);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x02);
            }

            if (GetKey(Key.I).Pressed || GetKey(Key.I).Down)
            {
                _keyLine[5] = _keyLine[5] & ~(0x04);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x04);
            }

            if (GetKey(Key.U).Pressed || GetKey(Key.U).Down)
            {
                _keyLine[5] = _keyLine[5] & ~(0x08);
            }
            else
            {
                _keyLine[5] = _keyLine[5] | (0x08);
            }

            if (GetKey(Key.Y).Pressed || GetKey(Key.Y).Down)
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

            if (GetKey(Key.L).Pressed || GetKey(Key.L).Down)
            {
                _keyLine[6] = _keyLine[6] & ~(0x02);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x02);
            }

            if (GetKey(Key.K).Pressed || GetKey(Key.K).Down)
            {
                _keyLine[6] = _keyLine[6] & ~(0x04);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x04);
            }

            if (GetKey(Key.J).Pressed || GetKey(Key.J).Down)
            {
                _keyLine[6] = _keyLine[6] & ~(0x08);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x08);
            }

            if (GetKey(Key.H).Pressed || GetKey(Key.H).Down)
            {
                _keyLine[6] = _keyLine[6] & ~(0x10);
            }
            else
            {
                _keyLine[6] = _keyLine[6] | (0x10);
            }

            if (GetKey(Key.Space).Pressed || GetKey(Key.Space).Down)
            {
                _keyLine[7] = _keyLine[7] & ~(0x1);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x1);
            }

            //Symbol Shift
            if (GetKey(Key.Control).Down)
            {
                _keyLine[7] = _keyLine[7] & ~(0x02);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x02);
            }

            if (GetKey(Key.M).Pressed || GetKey(Key.M).Down)
            {
                _keyLine[7] = _keyLine[7] & ~(0x04);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x04);
            }

            if (GetKey(Key.N).Pressed || GetKey(Key.N).Down)
            {
                _keyLine[7] = _keyLine[7] & ~(0x08);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x08);
            }

            if (GetKey(Key.B).Pressed || GetKey(Key.B).Down)
            {
                _keyLine[7] = _keyLine[7] & ~(0x10);
            }
            else
            {
                _keyLine[7] = _keyLine[7] | (0x010);
            }

            //Special Helper Mappings
            if (GetKey(Key.Back).Pressed || GetKey(Key.Delete).Pressed ||
                GetKey(Key.Back).Down || GetKey(Key.Delete).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[0] & ~(0x1);
            }

            if (GetKey(Key.Left).Pressed || GetKey(Key.Left).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[3] = _keyLine[3] & ~(0x10);
            }

            if (GetKey(Key.Right).Pressed || GetKey(Key.Right).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[4] & ~(0x04);
            }

            if (GetKey(Key.Up).Pressed || GetKey(Key.Up).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[4] & ~(0x08);
            }

            if (GetKey(Key.Down).Pressed || GetKey(Key.Down).Down)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[4] & ~(0x10);
            }

            return _keyLine;
        }

        private void SetUpMachineState(Z80Snapshot snapshot)
        {
            _cpu.A = (byte)(snapshot.AF >> 8);
            _cpu.F = (Z80.Flags)((byte)(snapshot.AF & 0x00FF));
            _cpu.A1 = (byte)(snapshot.AF1 >> 8);
            _cpu.F1 = (Z80.Flags)((byte)(snapshot.AF1 & 0x00FF));

            _cpu.B = (byte)(snapshot.BC >> 8);
            _cpu.C = (byte)(snapshot.BC & 0x00FF);
            _cpu.B1 = (byte)(snapshot.BC1 >> 8);
            _cpu.C1 = (byte)(snapshot.BC1 & 0x00FF);

            _cpu.D = (byte)(snapshot.DE >> 8);
            _cpu.E = (byte)(snapshot.DE & 0x00FF);
            _cpu.D1 = (byte)(snapshot.DE1 >> 8);
            _cpu.E1 = (byte)(snapshot.DE1 & 0x00FF);

            _cpu.H = (byte)(snapshot.HL >> 8);
            _cpu.L = (byte)(snapshot.HL & 0x00FF);
            _cpu.H1 = (byte)(snapshot.HL1 >> 8);
            _cpu.L1 = (byte)(snapshot.HL1 & 0x00FF);

            _cpu.IX = (ushort)snapshot.IX;
            _cpu.IY = (ushort)snapshot.IY;
            _cpu.SP = (ushort)snapshot.SP;
            _cpu.PC = (ushort)snapshot.PC;

            _cpu.IFF1 = snapshot.IFF1;
            _cpu.IFF2 = snapshot.IFF2;

            _cpu.I = snapshot.I;
            _cpu.R = snapshot.R;

            _cpu.InterruptMode = (InterruptMode)snapshot.IM;

            Array.Copy(snapshot.Spectrum48, 0, _ram, 16384, 49152);
        }
    }
}
