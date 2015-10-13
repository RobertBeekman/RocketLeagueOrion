using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using RocketLeagueOrion.Models;
using RocketLeagueOrion.Utilities.Logitech;

namespace RocketLeagueOrion.Controllers
{
    public class RocketLeagueController
    {
        public RocketLeagueController(MainController mainController)
        {
            // Setup Logitech SDK
            OrionController.SetupSdk();

            MainController = mainController;
            RocketLeagueWorker = new BackgroundWorker();
            RocketLeagueWorker.DoWork += RocketLeagueWorker_DoWork;
        }

        public MainController MainController { get; set; }
        public BackgroundWorker RocketLeagueWorker { get; set; }

        public Process GetProcessIfRunning()
        {
            var rlProcess = Process.GetProcessesByName("RocketLeague");
            return rlProcess.Length >= 1 ? rlProcess[0] : null;
        }

        private void RocketLeagueWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (!RocketLeagueWorker.CancellationPending)
            {
                // Refresh address every second
                if (sw.ElapsedMilliseconds > 1000)
                {
                    // Ensure process is still running
                    if (MainController.MainModel.RocketLeagueProcess.HasExited)
                    {
                        MainController.MainModel.Status = "Game not found";
                        LogitechGSDK.LogiLedRestoreLighting();
                        return;
                    }
                    sw.Restart();
                }

                // Update model boost amount
                MainController.MainModel.GetBoostAmount();
                FadeInIfHigher(MainController.MainModel);
                // Generate new bitmap
                var bitmap = CreateBoostBitmap(MainController.MainModel);

                // Post it to device
                LogitechGSDK.LogiLedSetLightingFromBitmap(OrionController.BitmapToByteArray(bitmap));
                Thread.Sleep(100);
            }
        }

        public Bitmap CreateBoostBitmap(MainModel mainModel)
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

        public void FadeInIfHigher(MainModel mainModel)
        {
            if (mainModel.BoostAmount <= mainModel.PreviousBoost)
                return;
            const int amountOfSteps = 6;

            var difference = mainModel.BoostAmount - mainModel.PreviousBoost;
            var differenceStep = difference / amountOfSteps;
            var differenceStepRest = difference % amountOfSteps;
            mainModel.BoostAmount = mainModel.PreviousBoost;

            for (var i = 0; i < amountOfSteps; i++)
            {
                if (differenceStepRest > 0)
                {
                    differenceStepRest -= 1;
                    mainModel.BoostAmount += 1;
                }
                mainModel.BoostAmount += differenceStep;

                var bitmap = CreateBoostBitmap(mainModel);
                LogitechGSDK.LogiLedSetLightingFromBitmap(OrionController.BitmapToByteArray(bitmap));
                Thread.Sleep(50);
            }
        }
    }
}