using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WindowsDevelopment_CQ17_31_Puzzles
{
    class GameHelper
    {
        public Image LoadedImage;
        PuzzleMaker maker;
        Timer timer;
        //public Image[,] pieces;

        int count = 60 * 3;

        public GameHelper()
        {
            maker = PuzzleMaker.GetInstance();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
        }

        public void NewGame()
        {
            count = 60 * 3;
            maker.GeneratePuzzle();
            
        }

        
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            count--;
            if (count == 0)
            {
                count = 60 * 3;
                bool isWIn = maker.CheckWin();

                if (isWIn != true)
                {
                    MessageBox.Show("Time out!!! You lose");
                }
                timer.Stop();
            }
        }

        public void LoadImage()
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {

                var source = new BitmapImage(
                    new Uri(screen.FileName));
                LoadedImage.Source = source;
            }
        }



    }
}
