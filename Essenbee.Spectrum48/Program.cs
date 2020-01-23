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
            _renderTicks = (int)((1.0f / 4000.0f) * Stopwatch.Frequency); // Tune this value, looking for 50 FPS
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
                    var snapshot = Z80FileReader.LoadZ80File("D:/Snapshots/Adventure.z80");

                    if (snapshot.Type == 0)
                    {
                        SetUpMachineState(snapshot);
                    }
                    else
                    {
                        Console.WriteLine("** Not a Spectrum 48K file! **");
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

            //Symbol Shift
            if (GetKey(Key.Control).Down)
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

            //Special Helper Mappings
            if (GetKey(Key.Back).Pressed || GetKey(Key.Delete).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[0] & ~(0x1);
            }

            if (GetKey(Key.Left).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[3] = _keyLine[3] & ~(0x10);
            }

            if (GetKey(Key.Right).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[4] & ~(0x04);
            }

            if (GetKey(Key.Up).Pressed)
            {
                _keyLine[0] = _keyLine[0] & ~(0x1);
                _keyLine[4] = _keyLine[4] & ~(0x08);
            }

            if (GetKey(Key.Down).Pressed)
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
