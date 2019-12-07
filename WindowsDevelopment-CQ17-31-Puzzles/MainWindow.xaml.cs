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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WindowsDevelopment_CQ17_31_Puzzles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PuzzleMaker maker;
        //Timer timer;
        //int count = 60 * 3;

        DispatcherTimer _timer;
        TimeSpan _time;

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
            //timer = new Timer();
            //timer.Interval = 1000;
            //timer.Elapsed += Timer_Elapsed;

            //_time = TimeSpan.FromMinutes(3);
            //_time = TimeSpan.FromSeconds(3);

            //_timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, 
            //    delegate
            //    {
            //        LabelTimer.Content = _time.ToString(@"mm\:ss");
            //        if (_time == TimeSpan.Zero)
            //        {
            //            System.Windows.MessageBox.Show("Time up", "You lose", MessageBoxButton.OK, MessageBoxImage.Information);
            //            _timer.Stop();
            //        }
            //        _time = _time.Add(TimeSpan.FromSeconds(-1));
            //    }, Application.Current.Dispatcher);

            //_timer.Start();

            //drawUI();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            maker.GeneratePuzzle();
            for (int i_tmp = 0; i_tmp < 3; i_tmp++)
            {
                for (int j_tmp = 0; j_tmp < 3; j_tmp++)
                {
                    int[,] _a = maker.PuzzleTags;
                    Debug.Write($"{_a[i_tmp, j_tmp]} ");
                }
                Debug.WriteLine("");
            }
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                leftBottomCanvas.Children.Clear();
                drawUI();
                SetupPieces(screen.FileName);
                //Set time
                _time = TimeSpan.FromMinutes(3);

                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                    delegate
                    {
                        LabelTimer.Content = _time.ToString(@"mm\:ss");
                        if (_time == TimeSpan.Zero)
                        {
                            System.Windows.MessageBox.Show("Time up", "You lose", MessageBoxButton.OK, MessageBoxImage.Error);
                            _timer.Stop();
                        }
                        _time = _time.Add(TimeSpan.FromSeconds(-1));
                    }, Application.Current.Dispatcher);

                _timer.Start();

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
                        //Debug.WriteLine($"{source.Width} - {source.Height}");
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
                        cropImage.Tag = i * rows + j;
                        cropImage.Name = "Piece" + (i * rows + j);
                       

                        Tuple<int, int> position = maker.GetPiecePosition(i * 3 + j);

                        Canvas.SetLeft(cropImage, startX + lineWeight/2 + position.Item2 * (w_fix + lineWeight));
                        Canvas.SetTop(cropImage, startY  + lineWeight/2 + position.Item1 * (h_fix + lineWeight));

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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isDragging)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        Tuple<int, int> emptyPiece_KeyLeft = maker.GetPiecePosition(8);

                        int i_KeyLeft = emptyPiece_KeyLeft.Item1;
                        int j_KeyLeft = emptyPiece_KeyLeft.Item2 + 1;
                        if (j_KeyLeft >= 0 && j_KeyLeft <= 2)
                        {
                            foreach (var child in leftBottomCanvas.Children)
                            {
                                if (child is Image)
                                {
                                    if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyLeft, j_KeyLeft])
                                    {
                                        selectedBitmap = child as Image;
                                        break;
                                    }
                                }
                            }

                            int from = startX + lineWeight / 2 + j_KeyLeft * (h_fix + lineWeight);
                            int to = startX + lineWeight / 2 + (j_KeyLeft - 1) * (h_fix + lineWeight);
                            movePieceAnimation(from, to, 1);
                            maker.MovePiece(new Tuple<int, int>(i_KeyLeft, j_KeyLeft), new Tuple<int, int>(i_KeyLeft, j_KeyLeft - 1));
                            if (maker.CheckWin())
                                System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        break;
                    case Key.Right:
                        Tuple<int, int> emptyPiece_KeyRight = maker.GetPiecePosition(8);

                        int i_KeyRight = emptyPiece_KeyRight.Item1;
                        int j_KeyRight = emptyPiece_KeyRight.Item2 - 1;
                        if (j_KeyRight >= 0 && j_KeyRight <= 2)
                        {

                            foreach (var child in leftBottomCanvas.Children)
                            {
                                if (child is Image)
                                {
                                    if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyRight, j_KeyRight])
                                    {
                                        selectedBitmap = child as Image;
                                        break;
                                    }
                                }
                            }

                            int from = startX + lineWeight / 2 + j_KeyRight * (h_fix + lineWeight);
                            int to = startX + lineWeight / 2 + (j_KeyRight + 1) * (h_fix + lineWeight);
                            movePieceAnimation(from, to, 1);
                            maker.MovePiece(new Tuple<int, int>(i_KeyRight, j_KeyRight), new Tuple<int, int>(i_KeyRight, j_KeyRight + 1));
                            if (maker.CheckWin())
                                System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case Key.Up:
                        Tuple<int, int> emptyPiece_KeyUp = maker.GetPiecePosition(8);

                        int i_KeyUp = emptyPiece_KeyUp.Item1 + 1;
                        int j_KeyUp = emptyPiece_KeyUp.Item2;
                        if (i_KeyUp >= 0 && i_KeyUp <= 2)
                        {
                            //Find piece to move      
                            foreach (var child in leftBottomCanvas.Children)
                            {
                                if (child is Image)
                                {
                                    if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyUp, j_KeyUp])
                                    {
                                        selectedBitmap = child as Image;
                                        break;
                                    }
                                }
                            }

                            //Set up and move piece
                            int from = startY + lineWeight / 2 + i_KeyUp * (h_fix + lineWeight);
                            int to = startY + lineWeight / 2 + (i_KeyUp - 1) * (h_fix + lineWeight);
                            movePieceAnimation(from, to, 2);
                            maker.MovePiece(new Tuple<int, int>(i_KeyUp, j_KeyUp), new Tuple<int, int>(i_KeyUp - 1, j_KeyUp));
                            if (maker.CheckWin())
                                System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case Key.Down:
                        Tuple<int, int> emptyPiece_KeyDown = maker.GetPiecePosition(8);

                        int i_KeyDown = emptyPiece_KeyDown.Item1 - 1;
                        int j_KeyDown = emptyPiece_KeyDown.Item2;
                        if (i_KeyDown >= 0 && i_KeyDown <= 2)
                        {
                            //Find piece to move      
                            foreach (var child in leftBottomCanvas.Children)
                            {
                                if (child is Image)
                                {
                                    if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyDown, j_KeyDown])
                                    {
                                        selectedBitmap = child as Image;
                                        break;
                                    }
                                }
                            }

                            //Set up and move piece
                            int from = startY + lineWeight / 2 + i_KeyDown * (h_fix + lineWeight);
                            int to = startY + lineWeight / 2 + (i_KeyDown + 1) * (h_fix + lineWeight);
                            movePieceAnimation(from, to, 2);
                            maker.MovePiece(new Tuple<int, int>(i_KeyDown, j_KeyDown), new Tuple<int, int>(i_KeyDown + 1, j_KeyDown));
                            if (maker.CheckWin())
                                System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                }
            }
        }
        /// <summary>
        ///     Move piece effect with animation
        ///     This function use selectedBitmap property as target to move
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="direction">
        ///     1: left to right
        ///     2: right to left
        ///     3: down
        ///     4: up
        /// </param>
        private void movePieceAnimation(int from, int to, int direction)
        {
            var animation = new DoubleAnimation();

            animation.From = from;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            var story = new Storyboard();

            story.Children.Add(animation);
            Storyboard.SetTargetName(animation, selectedBitmap.Name);
            Storyboard.SetTarget(animation, selectedBitmap);
            switch(direction)
            {
                case 1:
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
                    break;
                case 2:
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.TopProperty));
                    break;
     
            }
            story.Begin(this);
        }

        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var position = e.GetPosition(this);
            int i = (int)(position.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;
            int j = (int)(position.X - startX) / colWidth;

            if (i >= 0 && i <= rows - 1  && j >= 0 && j <= cols - 1 && isMoveValid())
            {
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + j * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + i * (h_fix + lineWeight));
                maker.MovePiece(
                    new Tuple<int, int>((int)(oldPosition_piece.Y - startY - leftTopCanvas.ActualHeight) / rowHeight, (int)(oldPosition_piece.X - startX) / colWidth),
                    new Tuple<int, int>(i, j)
                    );
                if (maker.CheckWin())
                    System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            // Snap to old position if piece is out of range
            else
            {
                i = (int)(oldPosition_piece.Y - startY - leftTopCanvas.ActualHeight) / rowHeight;
                j = (int)(oldPosition_piece.X - startX) / colWidth;
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + j * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + i * (h_fix + lineWeight));
            }
            //movePieceAnimation(0, 800, 1);
        }

        private bool isMoveValid()
        {
           
            Tuple<int, int> emptyPiece = maker.GetPiecePosition(8);
            Tuple<int, int> selectedBitmap_Position = maker.GetPiecePosition((int)selectedBitmap.Tag);
            if (emptyPiece.Item1 - 1 == selectedBitmap_Position.Item1 && emptyPiece.Item2 == selectedBitmap_Position.Item2)
                return true;
            if (emptyPiece.Item1 + 1 == selectedBitmap_Position.Item1 && emptyPiece.Item2 == selectedBitmap_Position.Item2)
                return true;
            if (emptyPiece.Item1 == selectedBitmap_Position.Item1 && emptyPiece.Item2 - 1 == selectedBitmap_Position.Item2)
                return true;
            if (emptyPiece.Item1 == selectedBitmap_Position.Item1 && emptyPiece.Item2 + 1 == selectedBitmap_Position.Item2)
                return true;   
            return false;
        }

        //private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    count--;
        //    if (count == 0)
        //    {
        //        count = 60 * 3;
        //        bool isWIn = maker.CheckWin();

        //        if (isWIn != true)
        //        {
        //            MessageBox.Show("Time out!!! You lose");
        //        }          
        //        timer.Stop();
        //    }
        //}

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
