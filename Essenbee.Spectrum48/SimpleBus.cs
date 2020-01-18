using System.Collections.Generic;
using PixelEngine;

namespace Essenbee.Z80.Spectrum48
{
    public class SimpleBus : IBus
    {
        public bool ScreenReady { get; set; }
        public bool Interrupt { get; set; }
        public bool NonMaskableInterrupt { get; set; }
        public IList<byte> Data { get; set; } = new List<byte>();

        private byte[] _memory;
        private int _lineRendered = 0;
        private int _pixelLine = 0;
        private int _screenLine = 0;
        private Sprite _theScreen = new Sprite(32 * 8, 24 * 8);
        private const ushort _screenStart = 0x4000;     // Beginning of Spectrum video RAM
        private const ushort _attributeStart = 0x5800;  // 768 bytes of colour attributes

        public SimpleBus(byte[] ram)
        {
            _memory = ram;
        }

        public IReadOnlyCollection<byte> RAM
        {
            get => _memory;
        }

        public byte Read(ushort addr, bool ro = false)
        {
            return _memory[addr];
        }

        public byte ReadPeripheral(ushort port)
        {
            return 0;
        }

        public void Write(ushort addr, byte data)
        {
            _memory[addr] = data;
        }

        public void WritePeripheral(ushort port, byte data)
        {
        }

        public Sprite GetScreen()
        {
            return _theScreen;
        }

        public void RunRenderer()
        {
            StepRenderer();
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

                // Decode bits
                for (var bitPos = 0; bitPos < 8; ++bitPos)
                {
                    var b0 = 0x1 & (c0 >> (7 - bitPos));
                    var b1 = 0x1 & (c1 >> (7 - bitPos));
                    var b2 = 0x1 & (c2 >> (7 - bitPos));
                    var x = (c * 8) + bitPos;

                    _theScreen[x, _lineRendered + 0] = b0 != 0 
                        ? Pixel.Presets.White
                        : Pixel.Presets.Blue;
                    _theScreen[x, _lineRendered + 64] = b1 != 0
                        ? Pixel.Presets.White
                        : Pixel.Presets.Blue;
                    _theScreen[x, _lineRendered + 128] = b2 != 0
                        ? Pixel.Presets.White
                        : Pixel.Presets.Blue;
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
    }
}
