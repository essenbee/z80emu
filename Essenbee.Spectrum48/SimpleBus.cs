using PixelEngine;
using System;
using System.Collections.Generic;

namespace Essenbee.Z80.Spectrum48
{
    public class SimpleBus : IBus
    {
        public bool ScreenReady { get; set; }
        public bool SoundOn { get; set; }
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
        private int _ink0, _ink1, _ink2;
        private int _paper0, _paper1, _paper2;
        private int _bright0, _bright1, _bright2;
        private int _flash0, _flash1, _flash2;
        private int _frameCounter;

        public SimpleBus(byte[] ram) => _memory = ram;

        public IReadOnlyCollection<byte> RAM
=> _memory;

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
            // Console.WriteLine($"OUT 0x{port:X4} Data 0x{data:X2}");

            //` Bit   7   6   5   4   3   2   1   0
            //`     +-------------------------------+
            //`     |   |   |   | E | M |   Border  |
            //`     +-------------------------------+

            // ULA select
            if ((port & 0x00FF) == 0xFE)
            {
                // Set border colour (0 - 7)
                if (port >> 8 < 8)
                {
                    var borderColour = data & 0b00000111;
                    BorderColour = GetColouredPixel(borderColour, 0);
                    return;
                }

                if ((port & 0b00001000) > 1)
                {
                    // Activate MIC
                }

                if ((port & 0b00010000) > 1)
                {
                    // Activate EAR
                    SoundOn = true;
                }
                else
                {
                    SoundOn = false;
                }
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

            for (var c = 0; c < 32; c++)
            {
                var c0 = _memory[memOffset0 + c]; // Get byte value 0
                var c1 = _memory[memOffset1 + c]; // Get byte value 1
                var c2 = _memory[memOffset2 + c]; // Get byte value 2

                ReadAttributesForCharacter(c);

                // Decode bits
                for (var bitPos = 0; bitPos < 8; ++bitPos)
                {
                    var b0 = 0x1 & (c0 >> (7 - bitPos));
                    var b1 = 0x1 & (c1 >> (7 - bitPos));
                    var b2 = 0x1 & (c2 >> (7 - bitPos));
                    var x = (c * 8) + bitPos;

                    _screenBuffer[x, _lineRendered + 0] = GetPixel(b0 != 0, _ink0, _paper0, _bright0, _flash0 == 1);
                    _screenBuffer[x, _lineRendered + 64] = GetPixel(b1 != 0, _ink1, _paper1, _bright1, _flash1 == 1);
                    _screenBuffer[x, _lineRendered + 128] = GetPixel(b2 != 0, _ink2, _paper2, _bright2, _flash2 == 1);
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
                _screenLine = 0; ;
            }

            if (_lineRendered > 63)
            {
                _lineRendered = 0;
                ScreenReady = true;
                _frameCounter++;
            }
        }

        private void ReadAttributesForCharacter(int c)
        {
            var offset = _attributeStart + (_screenLine << 5);
            var attrOffset0 = (0 << 8) + offset + c;
            var attrOffset1 = (1 << 8) + offset + c;
            var attrOffset2 = (2 << 8) + offset + c;

            var attr0 = _memory[attrOffset0];
            var attr1 = _memory[attrOffset1];
            var attr2 = _memory[attrOffset2];

            _ink0 = attr0 & 0b00000111;
            _paper0 = (attr0 & 0b00111000) >> 3;
            _bright0 = (attr0 & 0b01000000) >> 6;
            _flash0 = (attr0 & 0b10000000) >> 7;

            _ink1 = attr1 & 0b00000111;
            _paper1 = (attr1 & 0b00111000) >> 3;
            _bright1 = (attr1 & 0b01000000) >> 6;
            _flash1 = (attr1 & 0b10000000) >> 7;

            _ink2 = attr2 & 0b00000111;
            _paper2 = (attr2 & 0b00111000) >> 3;
            _bright2 = (attr2 & 0b01000000) >> 6;
            _flash2 = (attr2 & 0b10000000) >> 7;
        }
        private Pixel GetPixel(bool foreground, int ink, int paper, int brightness, bool flashing)
        {
            if (flashing)
            {
                if (_frameCounter < 17)
                {
                    return NormalPixel(foreground, ink, paper, brightness);
                }

                if (_frameCounter > 32)
                {
                    _frameCounter = 0;
                }

                return InvertedPixel(foreground, ink, paper, brightness);
            }

            return NormalPixel(foreground, ink, paper, brightness);
        }

        private Pixel InvertedPixel(bool foreground, int ink, int paper, int brightness) => 
            foreground ? GetColouredPixel(paper, brightness) : GetColouredPixel(ink, brightness);

        private Pixel NormalPixel(bool foreground, int ink, int paper, int brightness) => 
            foreground ? GetColouredPixel(ink, brightness) : GetColouredPixel(paper, brightness);

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
