using System;
using System.Diagnostics;
using System.Threading;
using RocketLeagueOrion.Models;
using RocketLeagueOrion.Utilities.Logitech;
using RocketLeagueOrion.Utilities.Memory;

namespace RocketLeagueOrion.Controllers
{
    public static class RocketLeagueController
    {
        public static Process GetProcessIfRunning()
        {
            var rlProcess = Process.GetProcessesByName("RocketLeague");
            return rlProcess.Length >= 1 ? rlProcess[0] : null;
        }

        public static void GetAddress(MainModel mainModel)
        {
            mainModel.Memory = new Memory(mainModel.RocketLeagueProcess);
            mainModel.BoostAddress = mainModel.Memory.GetAddress("\"RocketLeague.exe\"+01570020+94+204+6F4+21C");
        }

        public static void GetBoostAmount(MainModel mainModel)
        {
            var boostFloat = mainModel.Memory.ReadFloat(mainModel.BoostAddress)/3*100;
            mainModel.PreviousBoost = mainModel.BoostAmount;
            mainModel.BoostAmount = (int) Math.Round(boostFloat);
        }

        public static void FadeInIfHigher(MainModel mainModel)
        {
            if (mainModel.BoostAmount <= mainModel.PreviousBoost)
                return;

            var limit = mainModel.BoostAmount;
            var difference = mainModel.BoostAmount - mainModel.PreviousBoost;
            var differenceSteps = difference/6;
            mainModel.BoostAmount = mainModel.PreviousBoost;
            for (var i = mainModel.BoostAmount; i < limit; i += differenceSteps)
            {
                mainModel.BoostAmount += differenceSteps;
                var bitmap = OrionController.CreateBoostBitmap(mainModel);
                LogitechGSDK.LogiLedSetLightingFromBitmap(OrionController.BitmapToByteArray(bitmap));
                Thread.Sleep(50);
            }
        }
    }
}