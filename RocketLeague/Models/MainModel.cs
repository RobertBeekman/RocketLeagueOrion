using System;
using System.Diagnostics;
using System.Drawing;
using RocketLeagueOrion.Utilities.Memory;
using RocketLeagueOrion.Views;

namespace RocketLeagueOrion.Models
{
    public class MainModel
    {
        #region Backing fields
        // Game data
        private int _boostAmount;
        // GUI
        private string _status;
        #endregion

        public MainModel(MainView mainView)
        {
            MainView = mainView;
        }

        private MainView MainView { get; }
        public Process RocketLeagueProcess { get; set; }

        public IntPtr BoostAddress { get; set; }
        public int PreviousBoost { get; set; }

        // Colors
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        // GUI
        public int BoostAmount
        {
            get { return _boostAmount; }
            set
            {
                _boostAmount = value;
                MainView.labelBoostAmount.InvokeIfRequired(() => { MainView.labelBoostAmount.Text = value.ToString(); });
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                MainView.labelStatus.InvokeIfRequired(() => { MainView.labelStatus.Text = value; });
            }
        }

        public void GetBoostAmount()
        {
            var memory = new Memory(RocketLeagueProcess);
            BoostAddress = memory.GetAddress("\"RocketLeague.exe\"+0155B050+580+40+664+6F4+21C");

            var boostFloat = memory.ReadFloat(BoostAddress) * 100 / 3;
            PreviousBoost = BoostAmount;
            BoostAmount = (int)Math.Ceiling(boostFloat);
        }
    }
}