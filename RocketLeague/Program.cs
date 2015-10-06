using System;
using System.Windows.Forms;
using RocketLeagueOrion.Controllers;
using RocketLeagueOrion.Views;

namespace RocketLeagueOrion
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainController = new MainController(new MainView());
            Application.Run(mainController.MainView);
        }
    }
}