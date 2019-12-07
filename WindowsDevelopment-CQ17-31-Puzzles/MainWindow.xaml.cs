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
                        

                        Tuple<int, int> position = maker.GetPiecePosition(i * 3 + j);

                        cropImage.Tag = i * 3 + j;

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
            int i = (int)(position.X - startX) / colWidth;
            int j = (int)(position.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;

            Tuple<int, int> newPosition = new Tuple<int, int>(i, j);

            int oldX = (int)(oldPosition_piece.X - startX) / colWidth;
            int oldY = (int)(oldPosition_piece.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;

            Tuple<int, int> oldPosition = new Tuple<int, int>(oldX, oldY);

            if (maker.MovePiece(oldPosition, newPosition))
            {
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + i * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + j * (h_fix + lineWeight));
                bool isWIn = maker.CheckWin();
                if (isWIn)
                {
                    MessageBox.Show("You won!!!!");
                }
            }
            // Snap to old position if piece is out of range
            else
            {
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + oldX * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + oldY * (h_fix + lineWeight));
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

        private void Hint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DirectionButton_Click(object sender, RoutedEventArgs e)
        {
            Tuple<int, int> emptySpace = maker.GetPiecePosition(8);
            Tuple<int, int> chosenPiece = new Tuple<int, int>(0, 0);
            if (sender == UpButton)
            {
                chosenPiece = new Tuple<int, int>(emptySpace.Item1, emptySpace.Item2 + 1);
            }
            if (sender == DownButton)
            {
                chosenPiece = new Tuple<int, int>(emptySpace.Item1, emptySpace.Item2 - 1);
            }
            if (sender == LeftButton)
            {
                chosenPiece = new Tuple<int, int>(emptySpace.Item1 + 1, emptySpace.Item2);
            }
            if (sender == RighButton)
            {
                chosenPiece = new Tuple<int, int>(emptySpace.Item1 - 1, emptySpace.Item2);
            }

            if (chosenPiece.Item1 < 0 || chosenPiece.Item1 > cols - 1)
            {
                return;
            }

            if (chosenPiece.Item2 < 0 || chosenPiece.Item2 > rows - 1)
            {
                return;
            }

            //find selected image
            int ImageTag = maker.PieceOrder[chosenPiece.Item1, chosenPiece.Item2];
            selectedBitmap = findImageByTag(ImageTag);

            //move that image
            if (maker.MovePiece(chosenPiece, emptySpace))
            {
                
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + emptySpace.Item1 * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + emptySpace.Item2 * (h_fix + lineWeight));
                bool isWIn = maker.CheckWin();
                if (isWIn)
                {
                    MessageBox.Show("You won!!!!");
                }
            }
        }

        

        Image findImageByTag(int tag)
        {
            var images = leftBottomCanvas.Children.OfType<Image>().ToList();
            for (int i = 0; i < images.Count; i++)
            {
                var img = images[i];
                if (img.Tag.ToString() == tag.ToString())
                {
                    return img;
                }
            }
            return null;
        }




    }
}
