using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using RocketLeagueOrion.Utilities.Logitech;

namespace RocketLeagueOrion.Controllers
{
    public static class OrionController
    {
        public static void SetupSdk()
        {
            LogitechGSDK.LogiLedInit();
            Thread.Sleep(500);
            LogitechGSDK.LogiLedSaveCurrentLighting();
        }

        public static byte[] BitmapToByteArray(Bitmap b)
        {
            var rect = new Rectangle(0, 0, b.Width, b.Height);
            var bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);

            var depth = Image.GetPixelFormatSize(b.PixelFormat);
            var step = depth/8;
            var pixels = new byte[(21*6)*step];
            var iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);
            return pixels;
        }
    }
}