using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using RocketLeagueOrion.Models;
using RocketLeagueOrion.Properties;
using RocketLeagueOrion.Utilities.Logitech;
using RocketLeagueOrion.Views;

namespace RocketLeagueOrion.Controllers
{
    public class MainController
    {
        private readonly BackgroundWorker _processWorker;
        private readonly BackgroundWorker _updateWorker;

        public MainController(MainView mainView)
        {
            // Models
            MainModel = new MainModel(mainView)
            {
                // Load color settings
                MainColor = Settings.Default.MainColor,
                SecondaryColor = Settings.Default.SecondaryColor
            };

            // Views
            MainView = mainView;

            // Background workers
            _processWorker = new BackgroundWorker();
            _processWorker.DoWork += processWorker_DoWork;
            _processWorker.WorkerSupportsCancellation = true;

            _updateWorker = new BackgroundWorker();
            _updateWorker.DoWork += updateWorker_DoWork;

            // Grand view access to MainController
            MainView.MainController = this;

            // Start looking for RocketLeague.exe
            _processWorker.RunWorkerAsync();
        }

        // Models
        public MainModel MainModel { get; set; }
        // Views
        public MainView MainView { get; set; }

        private void processWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Check every 10 seconds to see wether Rocket League is running
            while (!_processWorker.CancellationPending)
            {
                var rlProcess = RocketLeagueController.GetProcessIfRunning();
                if (rlProcess != null)
                {
                    Thread.Sleep(500);

                    MainModel.RocketLeagueProcess = rlProcess;
                    MainModel.Status = "Game running";

                    _updateWorker.RunWorkerAsync();
                    _processWorker.CancelAsync();
                }
                else
                    MainModel.Status = "Game not found";

                Thread.Sleep(10000);
            }
        }

        private void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RocketLeagueController.GetAddress(MainModel);
            }
            catch (AccessViolationException)
            {
                _processWorker.RunWorkerAsync();
                return;
            }

            // Setup Logitech SDK
            OrionController.SetupSdk();

            var sw = new Stopwatch();
            sw.Start();

            while (!_updateWorker.CancellationPending)
            {
                // Refresh address every second
                if (sw.ElapsedMilliseconds > 1000)
                {
                    // Ensure process is still running
                    if (MainModel.RocketLeagueProcess.HasExited)
                    {
                        MainModel.Status = "Game not found";
                        LogitechGSDK.LogiLedRestoreLighting();
                        _processWorker.RunWorkerAsync();
                        return;
                    }
                    sw.Restart();
                }

                RocketLeagueController.GetAddress(MainModel);

                // Update model boost amount
                RocketLeagueController.GetBoostAmount(MainModel);
                RocketLeagueController.FadeInIfHigher(MainModel);

                // Generate new bitmap
                var bitmap = OrionController.CreateBoostBitmap(MainModel);

                // Post it to device
                LogitechGSDK.LogiLedSetLightingFromBitmap(OrionController.BitmapToByteArray(bitmap));
                Thread.Sleep(100);
            }
        }

        public void UpdateMainColor(Color color)
        {
            Settings.Default.MainColor = color;
            Settings.Default.Save();
            MainModel.MainColor = color;
        }

        public void UpdateSecondaryColor(Color color)
        {
            Settings.Default.SecondaryColor = color;
            Settings.Default.Save();
            MainModel.SecondaryColor = color;
        }

        public void Shutdown()
        {
            _processWorker.CancelAsync();
            LogitechGSDK.LogiLedRestoreLighting();
            LogitechGSDK.LogiLedShutdown();
        }
    }
}