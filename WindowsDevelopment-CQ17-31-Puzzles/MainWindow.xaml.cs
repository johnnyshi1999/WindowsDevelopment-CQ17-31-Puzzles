using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // height, width for cropped image in cell
        private int h_fix, w_fix;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rows = 3;
            cols = 3;
            rowHeight = 133;
            colWidth = 133;
            lineColor = "#BBADA0";
            lineWeight = 5;
            startX = 45;
            startY = 35;
            h_fix = rowHeight - lineWeight;
            w_fix = colWidth - lineWeight;

            maker = PuzzleMaker.GetInstance();
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;

            drawUI();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            maker.GeneratePuzzle();
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                leftBottomCanvas.Children.Clear();
                drawUI();
                SetupPieces(screen.FileName);
            }
        }

        public void SetupPieces(String fileName)
        {
            var source = new BitmapImage(
                    new Uri(fileName, UriKind.Absolute));
            //Uri uri = new Uri(fileName, UriKind.Absolute);
            //BitmapImage source = new BitmapImage();
            //source.BeginInit();
            //source.UriSource = uri;
            //source.DecodePixelWidth = (colWidth - lineWeight) * 3;
            //source.DecodePixelHeight = (rowHeight - lineWeight) * 3;
            //source.EndInit();
            PreviewImage.Source = source;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        Debug.WriteLine($"{source.Width} - {source.Height}");
                        var h = (int)(source.Height < source.Width ? source.Height : source.Width) / rows;
                        var w = (int)(source.Height < source.Width ? source.Height : source.Width) / cols;
   
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        var cropBitmap = new CroppedBitmap(source, rect);
                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Uniform;
                        cropImage.Width = h_fix;
                        cropImage.Height = w_fix;
                        cropImage.Source = cropBitmap;
                        leftBottomCanvas.Children.Add(cropImage);


                        cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;
                        cropImage.Tag = i * 3 + j;

                        Tuple<int, int> position = maker.GetPiecePosition(i * 3 + j);

                        Canvas.SetLeft(cropImage, startX + lineWeight/2 + position.Item1 * (w_fix + lineWeight));
                        Canvas.SetTop(cropImage, startY  + lineWeight/2 + position.Item2 * (h_fix + lineWeight));

                        //cropImage.MouseLeftButtonUp
                    }
                }
            }
        }

        bool isDragging = false;
        Image selectedBitmap = null;
        Point lastPosition_piece;
        Point oldPosition_piece;

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            selectedBitmap = sender as Image;      
            oldPosition_piece = e.GetPosition(this);
            lastPosition_piece = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            int i = (int)(position.Y - startY - leftTopCanvas.ActualHeight) / colWidth;
            int j = ((int)position.X - startX) / rowHeight;
            //this.Title = $"{position.X} - {position.Y}, a[{i}][{j}]";

            if (isDragging)
            {
                if (position.X <= leftBottomCanvas.ActualWidth && position.Y >= leftTopCanvas.ActualHeight )
                {
                    var dx = position.X - lastPosition_piece.X;
                    var dy = position.Y - lastPosition_piece.Y;

                    var lastLeft = Canvas.GetLeft(selectedBitmap);
                    var lastTop = Canvas.GetTop(selectedBitmap);

                    Canvas.SetLeft(selectedBitmap, lastLeft + dx);
                    Canvas.SetTop(selectedBitmap, lastTop + dy);
                    lastPosition_piece = position;
                }
                else
                {
                    Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + (int)(oldPosition_piece.X - startX) / colWidth * (w_fix + lineWeight));
                    Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + (int)(oldPosition_piece.Y - startY - leftTopCanvas.ActualHeight) / rowHeight * (h_fix + lineWeight));
                    isDragging = false;
                }
            }
        }

        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var position = e.GetPosition(this);
            int i = (int)(position.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;
            int j = (int)(position.X - startX) / colWidth;

            if (i >= 0 && i <= rows - 1  && j >= 0 && j <= cols - 1)
            {
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + j * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + i * (h_fix + lineWeight));
            }
            // Snap to old position if piece is out of range
            else
            {
                i = (int)(oldPosition_piece.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;
                j = (int)(oldPosition_piece.X - startX) / colWidth;
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + j * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + i * (h_fix + lineWeight));
            }
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

        private void drawUI()
        {
            // draw rows
            for (int i = 0; i <= rows; i++)
            {
                Line line_horizontal = new Line();
                line_horizontal.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
                line_horizontal.StrokeThickness = lineWeight;
                leftBottomCanvas.Children.Add(line_horizontal);
                line_horizontal.X1 = startX;
                line_horizontal.Y1 = startY + rowHeight * i;
                line_horizontal.X2 = startX + cols * colWidth;
                line_horizontal.Y2 = startY + rowHeight * i;
               
            }

            // draw cols
            for (int j = 0; j <= cols; j++)
            {
                Line line_vertical = new Line();
                line_vertical.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
                line_vertical.StrokeThickness = lineWeight;
                leftBottomCanvas.Children.Add(line_vertical);
                line_vertical.X1 = startX + colWidth * j;
                line_vertical.Y1 = startY;
                line_vertical.X2 = startX + colWidth * j;
                line_vertical.Y2 = startY + rows * rowHeight;
            }
        }
    }
}
