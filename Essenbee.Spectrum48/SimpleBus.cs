using System.Collections.Generic;
using PixelEngine;
using System;

namespace Essenbee.Z80.Spectrum48
{
    public class SimpleBus : IBus
    {
        public bool ScreenReady { get; set; }
        public Pixel BorderColour { get; set; } = Pixel.Presets.Black;

        public int[] KeyMatrix { get; set; } = new int[8];
        public bool Interrupt { get; set; }
        public bool NonMaskableInterrupt { get; set; }
        public IList<byte> Data { get; set; } = new List<byte>();

        private byte[] _memory;
        private int _lineRendered = 0;
        private int _pixelLine = 0;
        private int _screenLine = 0;
        private Sprite _screenBuffer = new Sprite(32 * 8, 24 * 8);
        private const ushort _screenStart = 0x4000;     // Beginning of Spectrum video RAM
        private const ushort _attributeStart = 0x5800;  // 768 bytes of colour attributes
        private int _pen0, _pen1, _pen2;
        private int _paper0, _paper1, _paper2;
        private int _bright0, _bright1, _bright2;

        public SimpleBus(byte[] ram)
        {
            _memory = ram;
        }

        public IReadOnlyCollection<byte> RAM
        {
            get => _memory;
        }

        public byte Read(ushort addr, bool ro = false) => _memory[addr];

        public byte ReadPeripheral(ushort port)
        {
            // Console.WriteLine($"IN 0x{port:X4}");

            if ((port & 1) == 0)
            {
                // Keyboard handling..
                byte result = 0xFF;
                var keyRow = (port & 0xFF00) >> 8;

                if (keyRow == 0x7F)
                {
                    result &= (byte)KeyMatrix[7];
                }

                if (keyRow == 0xBF)
                {
                    result &= (byte)KeyMatrix[6];
                }

                if (keyRow == 0xDF)
                {
                    result &= (byte)KeyMatrix[5];
                }

                if (keyRow == 0xEF)
                {
                    result &= (byte)KeyMatrix[4];
                }

                if (keyRow == 0xF7)
                {
                    result &= (byte)KeyMatrix[3];
                }

                if (keyRow == 0xFB)
                {
                    result &= (byte)KeyMatrix[2];
                }

                if (keyRow == 0xFD)
                {
                    result &= (byte)KeyMatrix[1];
                }

                if (keyRow == 0xFE)
                {
                    result &= (byte)KeyMatrix[0];
                }

                result &= 0x1F; //mask out bits 0 to 4
                result |= 0b11100000; //set bit 5 - 7
                //Console.WriteLine($">>>>>>> Sending {Convert.ToString(result ,2)}");

                return result;
            }

            return 0;
        }

        public void Write(ushort addr, byte data) => _memory[addr] = data;

        public void WritePeripheral(ushort port, byte data)
        {
            if (port == 0x07FE)
            {
                var borderColour = data & 0b00000111;
                BorderColour = GetColouredPixel(borderColour, 0);
            }
        }

        public Sprite GetScreen() => _screenBuffer;

        public void RunRenderer()
        {
            StepRenderer();

            if (ScreenReady)
            {
                Interrupt = true; // 50 Hz maskable interrupt
            }
        }

        private void StepRenderer()
        {
            var offset = _screenStart + (_pixelLine << 8) + (_screenLine << 5);
            // Top third of screen
            var memOffset0 = (0 << 11) + offset;
            // Middle third of screen
            var memOffset1 = (1 << 11) + offset;
            // Bottom third of screen
            var memOffset2 = (2 << 11) + offset;

            for (var c = 0; c < 32; ++c)
            {
                var c0 = _memory[memOffset0 + c]; // Get byte value 0
                var c1 = _memory[memOffset1 + c]; // Get byte value 1
                var c2 = _memory[memOffset2 + c]; // Get byte value 2

                if (_lineRendered % 8 == 0)
                {
                    ReadAttributesForCharacter(c);
                }

                // Decode bits
                for (var bitPos = 0; bitPos < 8; ++bitPos)
                {
                    var b0 = 0x1 & (c0 >> (7 - bitPos));
                    var b1 = 0x1 & (c1 >> (7 - bitPos));
                    var b2 = 0x1 & (c2 >> (7 - bitPos));
                    var x = (c * 8) + bitPos;

                    _screenBuffer[x, _lineRendered + 0] = b0 != 0 
                        ? GetColouredPixel(_pen0, _bright0)
                        : GetColouredPixel(_paper0, _bright0);
                    _screenBuffer[x, _lineRendered + 64] = b1 != 0
                        ? GetColouredPixel(_pen1, _bright1)
                        : GetColouredPixel(_paper1, _bright1);
                    _screenBuffer[x, _lineRendered + 128] = b2 != 0
                        ? GetColouredPixel(_pen2, _bright2)
                        : GetColouredPixel(_paper2, _bright2);
                }
            }

            _lineRendered++;
            _pixelLine++;

            if (_pixelLine > 7)
            {
                _pixelLine = 0;
                _screenLine++;
            }

            if (_screenLine > 7)
            {
                _screenLine = 0;;
            }

            if (_lineRendered > 63)
            {
                _lineRendered = 0;
                ScreenReady = true;
            }
        }

        private void ReadAttributesForCharacter(int c)
        {
            var attrib0 = _memory[_attributeStart + (_lineRendered / 8) + c];
            var attrib1 = _memory[_attributeStart + ((_lineRendered + 64) / 8) + c];
            var attrib2 = _memory[_attributeStart + ((_lineRendered + 128) / 8) + c];

            _pen0 = attrib0 & 0b00000111;
            _paper0 = (attrib0 & 0b00111000) >> 3;
            _bright0 = (attrib0 & 0b01000000) >> 6;

            _pen1 = attrib1 & 0b00000111;
            _paper1 = (attrib1 & 0b00111000) >> 3;
            _bright1 = (attrib1 & 0b01000000) >> 6;

            _pen2 = attrib2 & 0b00000111;
            _paper2 = (attrib2 & 0b00111000) >> 3;
            _bright2 = (attrib2 & 0b01000000) >> 6;
        }

        private Pixel GetColouredPixel(int colour, int brightness)
        {
            if (brightness == 0)
            {
                switch (colour)
                {
                    case 0: return Pixel.Presets.Black;
                    case 1: return Pixel.Presets.DarkBlue;
                    case 2: return Pixel.Presets.DarkRed;
                    case 3: return Pixel.Presets.DarkMagenta;
                    case 4: return Pixel.Presets.DarkGreen;
                    case 5: return Pixel.Presets.DarkCyan;
                    case 6: return Pixel.Presets.DarkYellow;
                    default: return Pixel.Presets.Grey;
                }
            }
            else 
            {
                switch (colour)
                {
                    case 0: return Pixel.Presets.Black;
                    case 1: return Pixel.Presets.Blue;
                    case 2: return Pixel.Presets.Red;
                    case 3: return Pixel.Presets.Magenta;
                    case 4: return Pixel.Presets.Green;
                    case 5: return Pixel.Presets.Cyan;
                    case 6: return Pixel.Presets.Yellow;
                    default: return Pixel.Presets.White;
                }
            }
        }
    }
}
