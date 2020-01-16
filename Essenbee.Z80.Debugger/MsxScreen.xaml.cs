using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Essenbee.Z80.Debugger
{
  public partial class MsxScreen : UserControl
  {
    const int screen  = 0x4000;
    const int width   = 256;
    const int height  = 192;
    const int stride  = 3;
    DispatcherTimer _refreshTimer;
    WriteableBitmap _screenBitmap;
    byte[]          _screenPixels;

    partial void Constructed__MsxScreen()
    {
      InitializeComponent();
      _refreshTimer = new DispatcherTimer(
          TimeSpan.FromMilliseconds(200)
        , DispatcherPriority.ApplicationIdle
        , OnRefresh
        , Dispatcher                 
        );
      _screenBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
      _screenPixels = new byte[width*height*stride];
      img.Source = _screenBitmap;

      _refreshTimer.Start();
    }

    void OnRefresh(object sender, EventArgs e)
    {
      // NOOOOOOO!!!!! I don't want to go through linq to get the array :(
      var memory = RawMemory?.ToArray();  

      if (memory != null && memory.Length >= screen + 0x1800)
      {
        // The layout of MSX graphics memory is a bit weird
        for (var s = 0; s < 3; ++s)
        {
          for (var cl = 0; cl < 8; ++cl)
          {
            for (var l = 0; l < 8; ++l)
            {
              var memOffset = screen + (s << 11) + (cl << 8) + (l << 5);
              for (var cx = 0; cx < 32; ++cx)
              {
                var c = memory[memOffset + cx];
                var bitmapOffset = stride*(((s << 11) + (l << 8) + (cl << 5) + cx) << 3);
                for (int x = 0, strideX = 0; x < 8; ++x, strideX += stride)
                {
                  var b = 0x1 & (c >> (7 - x));
                  // TODO: Add support for attribute area
                  if (b != 0)
                  {
                    _screenPixels[bitmapOffset + strideX + 0] = 0xFF;
                    _screenPixels[bitmapOffset + strideX + 1] = 0xFF;
                    _screenPixels[bitmapOffset + strideX + 2] = 0xFF;
                  }
                  else
                  {
                    _screenPixels[bitmapOffset + strideX + 0] = 0x00;
                    _screenPixels[bitmapOffset + strideX + 1] = 0x00;
                    _screenPixels[bitmapOffset + strideX + 2] = 0x00;
                  }
                }
              }
            }
          }
        }
      }
      _screenBitmap.WritePixels(new Int32Rect(0, 0, width, height), _screenPixels, width*stride, 0);
    }



  }
}
