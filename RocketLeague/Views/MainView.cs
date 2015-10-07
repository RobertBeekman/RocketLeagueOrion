using System;
using System.Drawing;
using System.Windows.Forms;
using RocketLeagueOrion.Controllers;

namespace RocketLeagueOrion.Views
{
    public partial class MainView : Form
    {
        private readonly Graphics _graphics;

        public MainView()
        {
            InitializeComponent();
            _graphics = panel1.CreateGraphics();
        }

        public MainController MainController { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = colorDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            MainController.UpdateMainColor(colorDialog1.Color);
            DrawFakeBmp();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = colorDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            MainController.UpdateSecondaryColor(colorDialog1.Color);
            DrawFakeBmp();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawFakeBmp();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainController.Shutdown();
        }

        private void DrawFakeBmp()
        {
            var oldBoost = MainController.MainModel.BoostAmount;
            MainController.MainModel.BoostAmount = 100;
            var bitmap = MainController.RocketLeagueController.CreateBoostBitmap(MainController.MainModel);
            MainController.MainModel.BoostAmount = oldBoost;

            _graphics.Clear(Color.Transparent);
            _graphics.DrawImage(bitmap, 0, 0, panel1.Width, panel1.Height*2);
        }
    }
}