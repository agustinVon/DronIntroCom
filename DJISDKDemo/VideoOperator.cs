using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace DJISDKDemo
{
    class VideoOperator
    {
        public void processVideoToImage(byte[] bytes, int height, int width)
        {
            WriteableBitmap bitmap = new WriteableBitmap(height, width);
            bitmap.FromByteArray(bytes);
        }
    }
}
