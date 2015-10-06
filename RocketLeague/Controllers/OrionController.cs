using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using RocketLeagueOrion.Models;
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
            // Disable keys that SetLightingFromBitmap can't access
            LogitechGSDK.LogiLedSetLighting(0, 0, 0);
        }

        public static Bitmap CreateBoostBitmap(MainModel mainModel)
        {
            // Orion bitmaps are 21 wide, 6 high.
            var flag = new Bitmap(21, 6);

            using (var g = Graphics.FromImage(flag))
            {
                g.Clear(Color.Transparent);
                var width = (int) (flag.Width/100.00*mainModel.BoostAmount);
                if (width <= 0)
                    return flag;

                var mainBrush = new LinearGradientBrush(new Rectangle(0, 0, width, flag.Height), mainModel.MainColor,
                    mainModel.SecondaryColor,
                    LinearGradientMode.Horizontal);
                g.FillRectangle(mainBrush, 0, 0, width, flag.Height);

                var endBrush = new LinearGradientBrush(new Rectangle(width - 2, 0, 4, flag.Height),
                    mainModel.SecondaryColor,
                    Color.Transparent, LinearGradientMode.Horizontal);
                g.FillRectangle(endBrush, width - 2, 0, 4, flag.Height);
            }
            return flag;
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