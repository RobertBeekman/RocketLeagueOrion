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

            // Controllers
            RocketLeagueController = new RocketLeagueController(this);

            // Background workers
            _processWorker = new BackgroundWorker();
            _processWorker.DoWork += processWorker_DoWork;
            _processWorker.WorkerSupportsCancellation = true;

            // Grand view access to MainController
            MainView.MainController = this;

            // Start looking for RocketLeague.exe
            _processWorker.RunWorkerAsync();
        }

        public RocketLeagueController RocketLeagueController { get; set; }

        // Models
        public MainModel MainModel { get; set; }
        // Views
        public MainView MainView { get; set; }

        private void processWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Check every 10 seconds to see wether Rocket League is running
            while (!_processWorker.CancellationPending)
            {
                // If the worker is busy, stop iteration
                if (RocketLeagueController.RocketLeagueWorker.IsBusy)
                {
                    Thread.Sleep(10000);
                    continue;
                }

                // If worker not busy and RL is running, start worker
                var rlProcess = RocketLeagueController.GetProcessIfRunning();
                if (rlProcess != null)
                {
                    Thread.Sleep(500);

                    MainModel.RocketLeagueProcess = rlProcess;
                    MainModel.Status = "Game running";

                    RocketLeagueController.RocketLeagueWorker.RunWorkerAsync();
                }
                // If worker not busy and RL not running, try again in 10 sec
                else
                    MainModel.Status = "Game not found";
                
                Thread.Sleep(10000);
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