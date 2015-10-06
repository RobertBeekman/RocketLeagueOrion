using System;
using System.Diagnostics;
using System.Drawing;
using RocketLeagueOrion.Utilities.Memory;
using RocketLeagueOrion.Views;

namespace RocketLeagueOrion.Models
{
    public class MainModel
    {
        // Game data
        private int _boostAmount;
        // GUI
        private string _status;

        public MainModel(MainView mainView)
        {
            MainView = mainView;
        }

        // Internal stuff
        private MainView MainView { get; }
        public Process RocketLeagueProcess { get; set; }
        public Memory Memory { get; set; }
        public IntPtr BoostAddress { get; set; }

        // Game data
        public int BoostAmount
        {
            get { return _boostAmount; }
            set
            {
                // Auto-update GUI
                _boostAmount = value;
                MainView.labelBoostAmount.InvokeIfRequired(() => { MainView.labelBoostAmount.Text = value.ToString(); });
            }
        }

        public int PreviousBoost { get; set; }

        // Colors
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        // GUI
        public string Status
        {
            get { return _status; }
            set
            {
                // Auto-update GUI
                _status = value;
                MainView.labelStatus.InvokeIfRequired(() => { MainView.labelStatus.Text = value; });
            }
        }
    }
}