using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsDevelopment_CQ17_31_Puzzles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PuzzleMaker maker;
        Timer timer;
        int count = 60 * 3;

        public MainWindow()
        {
            InitializeComponent();
        }

        // UI data
        private int rows, cols;
        private int rowHeight, colWidth;
        private string lineColor;
        private int lineWeight;
        private int startX;
        private int startY;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rows = 3;
            cols = 3;
            rowHeight = 133;
            colWidth = 133;
            lineColor = "#BBADA0";
            lineWeight = 5;
            startX = 40;
            startY = 195;

            maker = PuzzleMaker.GetInstance();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;

            //drawUI();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            maker.GeneratePuzzle();
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {

                SetupPieces(screen.FileName);
            }
        }

        public void SetupPieces(String fileName)
        {
            var source = new BitmapImage(
                    new Uri(fileName, UriKind.Absolute));
            PreviewImage.Source = source;

            for (int i = 0; i < 3; i++)
            {


                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        var h = (int)source.Height / 3;
                        var w = (int)source.Height / 3;
                        //Debug.WriteLine($"Len = {len}");
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        var cropBitmap = new CroppedBitmap(source,
                            rect);

                        var cropImage = new Image();
                        //cropImage.Stretch = Stretch.Uniform;
                        cropImage.Width = h;
                        cropImage.Height = w;
                        cropImage.Source = cropBitmap;
                        leftCanvas.Children.Add(cropImage);


                        cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                        cropImage.Tag = i * 3 + j;

                        Tuple<int, int> position = maker.GetPiecePosition(i * 3 + j);

                        Canvas.SetLeft(cropImage, startX + position.Item1 * (w));
                        Canvas.SetTop(cropImage, startY + position.Item2 * (h));

                        //cropImage.MouseLeftButtonUp
                    }
                }
            }
        }



        

        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
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

        //private void drawUI()
        //{
        //    // draw rows
        //    for (int i = 0; i <= rows; i++)
        //    {
        //        Line line_horizontal = new Line();
        //        line_horizontal.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
        //        line_horizontal.StrokeThickness = lineWeight;
        //        leftCanvas.Children.Add(line_horizontal);
        //        line_horizontal.X1 = startX;
        //        line_horizontal.Y1 = startY + rowHeight * i;
        //        line_horizontal.X2 = startX + cols * colWidth;
        //        line_horizontal.Y2 = startY + rowHeight * i;
        //    }

        //    // draw cols
        //    for (int j = 0; j <= cols; j++)
        //    {
        //        Line line_vertical = new Line();
        //        line_vertical.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
        //        line_vertical.StrokeThickness = lineWeight;
        //        leftCanvas.Children.Add(line_vertical);
        //        line_vertical.X1 = startX + colWidth * j;
        //        line_vertical.Y1 = startY;
        //        line_vertical.X2 = startX + colWidth * j;
        //        line_vertical.Y2 = startY + rows * rowHeight;
        //    }
        //}

        
    }
}
