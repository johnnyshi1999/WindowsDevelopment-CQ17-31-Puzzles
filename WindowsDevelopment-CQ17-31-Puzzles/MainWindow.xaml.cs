using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        //Gameplay vars
        bool isGameStarted = false;
        string currentImagePath = null;
        BitmapImage currentImage = null;
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

            //Initialize states of UI Components
            SaveGameButton.IsEnabled = false;
            leftBottomCanvas.IsEnabled = false;


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

        private void resetTimer(double timeInMiliseconds)
        {
            //Set time
            _time = TimeSpan.FromMilliseconds(timeInMiliseconds);
            LabelTimer.Content = _time.ToString(@"mm\:ss");

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                delegate
                {
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                    LabelTimer.Content = _time.ToString(@"mm\:ss");
                    if (_time == TimeSpan.Zero)
                    {
                        _timer.Stop();
                        leftBottomCanvas.IsEnabled = false;
                        System.Windows.MessageBox.Show("Time up", "You lose", MessageBoxButton.OK, MessageBoxImage.Error);
                        resetVariables();
                        return;
                    }
                }, System.Windows.Application.Current.Dispatcher);
        }

        private void InitGame(string pictureName, double timeInMiliseconds)
        {
            leftBottomCanvas.IsEnabled = true;
            leftBottomCanvas.Children.Clear();
            drawUI();
            SetupPieces(pictureName);

            resetTimer(timeInMiliseconds);

            //Set up flow
            currentImagePath = pictureName;
            isGameStarted = true;
            SaveGameButton.IsEnabled = true;
            LoadGameButton.Content = "Reset Game";
            StartGameButton.Content = "Exit Game";
        }

        private void ExitGame_Click()
        {
            leftBottomCanvas.IsEnabled = false;
            resetVariables();
        }

        private void ResetGame_Click()
        {
            //Reset Timer
            resetTimer(180000);
            //Generate new puzzle
            maker.GeneratePuzzle();
            //Clear
            leftBottomCanvas.Children.RemoveRange(rows + cols + 2, leftBottomCanvas.Children.Count - rows - 1 - cols - 1);
            //Add picture
            AddPictureToCanvas(currentImage);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (isGameStarted)
            {
                _timer.Stop();
                ExitGame_Click();
                return;
            }

            DifficultyDialog dialog = new DifficultyDialog();
            if (dialog.ShowDialog() == true)
            {
                maker.Difficulty = dialog.Difficulty;
            }
            else
            {
                maker.Difficulty = PuzzleMaker.Medium;
            }

            maker.GeneratePuzzle();
           
            var screen = new Microsoft.Win32.OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                InitGame(screen.FileName, 180000);
            }
        }

        private void AddPictureToCanvas(BitmapImage source)
        {
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

                        Canvas.SetLeft(cropImage, startX + lineWeight / 2 + position.Item1 * (w_fix + lineWeight));
                        Canvas.SetTop(cropImage, startY + lineWeight / 2 + position.Item2 * (h_fix + lineWeight));

                        //cropImage.MouseLeftButtonUp
                    }
                }
            }
        }

        public void SetupPieces(string fileName)
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
            currentImage = source;
            PreviewImage.Source = source;
            AddPictureToCanvas(source);
        }

        bool isDragging = false;
        bool isMoving = false;
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

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!isDragging && !isMoving)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        {
                            LeftButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                            break;
                        }
                    case Key.Right:
                        {
                            RighButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                            break;
                        }
                    case Key.Up:
                        {
                            UpButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                            break;
                        }
                    case Key.Down:
                        {
                            DownButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                            break;
                        }
                }
            }
            //if (!isDragging)
            //{
            //    switch (e.Key)
            //    {
            //        case Key.Left:
            //            Tuple<int, int> emptyPiece_KeyLeft = maker.GetPiecePosition(8);

            //            int i_KeyLeft = emptyPiece_KeyLeft.Item1;
            //            int j_KeyLeft = emptyPiece_KeyLeft.Item2 + 1;
            //            if (j_KeyLeft >= 0 && j_KeyLeft <= 2)
            //            {
            //                foreach (var child in leftBottomCanvas.Children)
            //                {
            //                    if (child is Image)
            //                    {
            //                        if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyLeft, j_KeyLeft])
            //                        {
            //                            selectedBitmap = child as Image;
            //                            break;
            //                        }
            //                    }
            //                }

            //                int from = startX + lineWeight / 2 + j_KeyLeft * (h_fix + lineWeight);
            //                int to = startX + lineWeight / 2 + (j_KeyLeft - 1) * (h_fix + lineWeight);
            //                movePieceAnimation(from, to, 1);
            //                maker.MovePiece(new Tuple<int, int>(i_KeyLeft, j_KeyLeft), new Tuple<int, int>(i_KeyLeft, j_KeyLeft - 1));
            //                if (maker.CheckWin())
            //                    System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
            //            }

            //            break;
            //        case Key.Right:
            //            Tuple<int, int> emptyPiece_KeyRight = maker.GetPiecePosition(8);

            //            int i_KeyRight = emptyPiece_KeyRight.Item1;
            //            int j_KeyRight = emptyPiece_KeyRight.Item2 - 1;
            //            if (j_KeyRight >= 0 && j_KeyRight <= 2)
            //            {

            //                foreach (var child in leftBottomCanvas.Children)
            //                {
            //                    if (child is Image)
            //                    {
            //                        if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyRight, j_KeyRight])
            //                        {
            //                            selectedBitmap = child as Image;
            //                            break;
            //                        }
            //                    }
            //                }

            //                int from = startX + lineWeight / 2 + j_KeyRight * (h_fix + lineWeight);
            //                int to = startX + lineWeight / 2 + (j_KeyRight + 1) * (h_fix + lineWeight);
            //                movePieceAnimation(from, to, 1);
            //                maker.MovePiece(new Tuple<int, int>(i_KeyRight, j_KeyRight), new Tuple<int, int>(i_KeyRight, j_KeyRight + 1));
            //                if (maker.CheckWin())
            //                    System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
            //            }
            //            break;
            //        case Key.Up:
            //            Tuple<int, int> emptyPiece_KeyUp = maker.GetPiecePosition(8);

            //            int i_KeyUp = emptyPiece_KeyUp.Item1 + 1;
            //            int j_KeyUp = emptyPiece_KeyUp.Item2;
            //            if (i_KeyUp >= 0 && i_KeyUp <= 2)
            //            {
            //                //Find piece to move      
            //                foreach (var child in leftBottomCanvas.Children)
            //                {
            //                    if (child is Image)
            //                    {
            //                        if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyUp, j_KeyUp])
            //                        {
            //                            selectedBitmap = child as Image;
            //                            break;
            //                        }
            //                    }
            //                }

            //                //Set up and move piece
            //                int from = startY + lineWeight / 2 + i_KeyUp * (h_fix + lineWeight);
            //                int to = startY + lineWeight / 2 + (i_KeyUp - 1) * (h_fix + lineWeight);
            //                movePieceAnimation(from, to, 2);
            //                maker.MovePiece(new Tuple<int, int>(i_KeyUp, j_KeyUp), new Tuple<int, int>(i_KeyUp - 1, j_KeyUp));
            //                if (maker.CheckWin())
            //                    System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
            //            }
            //            break;
            //        case Key.Down:
            //            Tuple<int, int> emptyPiece_KeyDown = maker.GetPiecePosition(8);

            //            int i_KeyDown = emptyPiece_KeyDown.Item1 - 1;
            //            int j_KeyDown = emptyPiece_KeyDown.Item2;
            //            if (i_KeyDown >= 0 && i_KeyDown <= 2)
            //            {
            //                //Find piece to move      
            //                foreach (var child in leftBottomCanvas.Children)
            //                {
            //                    if (child is Image)
            //                    {
            //                        if ((int)(child as Image).Tag == maker.PuzzleTags[i_KeyDown, j_KeyDown])
            //                        {
            //                            selectedBitmap = child as Image;
            //                            break;
            //                        }
            //                    }
            //                }

            //                //Set up and move piece
            //                int from = startY + lineWeight / 2 + i_KeyDown * (h_fix + lineWeight);
            //                int to = startY + lineWeight / 2 + (i_KeyDown + 1) * (h_fix + lineWeight);
            //                movePieceAnimation(from, to, 2);
            //                maker.MovePiece(new Tuple<int, int>(i_KeyDown, j_KeyDown), new Tuple<int, int>(i_KeyDown + 1, j_KeyDown));
            //                if (maker.CheckWin())
            //                    System.Windows.MessageBox.Show("Congratulation", "You win", MessageBoxButton.OK, MessageBoxImage.Information);
            //            }
            //            break;
            //    }
            //}
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
        private void movePieceAnimation(Tuple<int, int> oldPos, Tuple<int, int> newPos/*int from, int to, int direction*/)
        {
            
            //var animation = new DoubleAnimation();

            //animation.From = from;
            //animation.To = to;
            //animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            //var story = new Storyboard();

            //story.Children.Add(animation);
            //Storyboard.SetTargetName(animation, selectedBitmap.Name);
            //Storyboard.SetTarget(animation, selectedBitmap);
            //switch(direction)
            //{
            //    case 1:
            //        Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
            //        break;
            //    case 2:
            //        Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.TopProperty));
            //        break;

            //}
            //story.Begin(this);

            var horiztontalMove = new DoubleAnimation();
            horiztontalMove.From = startX + lineWeight / 2 + oldPos.Item1 * (w_fix + lineWeight);
            horiztontalMove.To = startX + lineWeight / 2 + newPos.Item1 * (w_fix + lineWeight);
            horiztontalMove.Duration = new Duration(TimeSpan.FromSeconds(0.2));

            var verticalMove = new DoubleAnimation();
            verticalMove.From = startY + lineWeight / 2 + oldPos.Item2 * (h_fix + lineWeight);
            verticalMove.To = startY + lineWeight / 2 + newPos.Item2 * (h_fix + lineWeight);
            verticalMove.Duration = new Duration(TimeSpan.FromSeconds(0.2));

            var story = new Storyboard();

            story.Children.Add(horiztontalMove);
            story.Children.Add(verticalMove);

            //Storyboard.SetTargetName(horiztontalMove, selectedBitmap.Name);
            //Storyboard.SetTargetName(verticalMove, selectedBitmap.Name);

            Storyboard.SetTarget(horiztontalMove, selectedBitmap);
            Storyboard.SetTarget(verticalMove, selectedBitmap);

            Storyboard.SetTargetProperty(horiztontalMove, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTargetProperty(verticalMove, new PropertyPath(Canvas.TopProperty));


            story.FillBehavior = FillBehavior.Stop;

            Tuple<int, int> newXY = new Tuple<int, int>((int)horiztontalMove.To, (int)verticalMove.To);

            // pass parameter to Story_Complete
            story.Completed += delegate (object sender, EventArgs e)
            {
                Story_Complete(sender, e, newXY);
            };
            story.Begin(this);
            



            //story.Completed += completed;


            //int newX = startX + lineWeight / 2 + newPos.Item1 * (w_fix + lineWeight);


            //int newY = startY + lineWeight / 2 + newPos.Item2 * (h_fix + lineWeight);

            //DoubleAnimation anim1 = new DoubleAnimation(0, newX - Canvas.GetLeft(selectedBitmap), TimeSpan.FromSeconds(0.5));
            //DoubleAnimation anim2 = new DoubleAnimation(0, newY - Canvas.GetTop(selectedBitmap), TimeSpan.FromSeconds(0.5));


            //TranslateTransform trans = new TranslateTransform();

            //selectedBitmap.RenderTransform = trans;

            //trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            //trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            //trans.BeginAnimation(TranslateTransform.XProperty, null);
            //trans.BeginAnimation(TranslateTransform.YProperty, null);
        }

        private void Story_Complete(object sender, EventArgs e, Tuple<int, int> newXY)
        {
            Canvas.SetLeft(selectedBitmap, newXY.Item1);
            Canvas.SetTop(selectedBitmap, newXY.Item2);
            isMoving = false;

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
                    _timer.Stop();
                    leftBottomCanvas.IsEnabled = false;
                    System.Windows.MessageBox.Show("You won!!!!");
                    resetVariables();
                }
            }
            // Snap to old position if piece is out of range
            else
            {
                Canvas.SetLeft(selectedBitmap, startX + lineWeight / 2 + oldX * (w_fix + lineWeight));
                Canvas.SetTop(selectedBitmap, startY + lineWeight / 2 + oldY * (h_fix + lineWeight));
            }
            //movePieceAnimation(0, 800, 1);
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

        private void SaveGame_Click(object sender, RoutedEventArgs e)
        {
            if (!isGameStarted) return;

            _timer.Stop();

            var screen = new FolderBrowserDialog();

            if (screen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = screen.SelectedPath + "\\Puzzles.psf";

                var writer = new StreamWriter(path, false);
               
                var currentState = maker.PieceOrder;

                StringBuilder fileContentString = new StringBuilder();
                // Image file
                fileContentString.Append(currentImagePath);
                fileContentString.Append("\n");

                //Current time
                fileContentString.Append(_time.TotalMilliseconds.ToString());
                fileContentString.Append("\n");

                // Game state
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        fileContentString.Append($"{currentState[i, j]}");
                        if (j != cols - 1)
                        {
                            fileContentString.Append(" ");
                        }
                    }
                    fileContentString.Append("\n");
                }

                writer.Write(Convert.ToBase64String(Encoding.ASCII.GetBytes(fileContentString.ToString())));

                writer.Close();

                System.Windows.MessageBox.Show("Game is saved");
            }

            _timer.Start();
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            if (isGameStarted)
            {
                _timer.Stop();
                ResetGame_Click();
                return;
            }

            var screen = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false
            };

            if(screen.ShowDialog() == true)
            {
                var fileInfo = screen.SafeFileName;
                if(!fileInfo.Equals("Puzzles.psf"))
                {
                    System.Windows.MessageBox.Show("FileName Invalid");
                    return;
                }
                var path = screen.FileName;

                var fileContent = File.ReadAllText(path);
                byte[] byteArray = Convert.FromBase64String(fileContent);

                MemoryStream stream = new MemoryStream(byteArray);

                // convert stream to string
                StreamReader reader = new StreamReader(stream);

                //Get image path
                string imagePath = reader.ReadLine();

                //If file doesn't exists
                if(!File.Exists(imagePath))
                {
                    System.Windows.MessageBox.Show("Image from save file does not exist, please recheck :3 ...", "Image not found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Get time
                double currentTime = double.Parse(reader.ReadLine());

                //Get state of gameboard
                int[,] tempState = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    var tokens = reader.ReadLine().Split(
                        new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < cols; j++)
                    {
                        tempState[i, j] = int.Parse(tokens[j]);
                    }
                }

                maker.LoadPuzzle(tempState);

                //Init board
                InitGame(imagePath, currentTime);
            }
        }

        private void Hint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            phase currentPhase = maker.GetCurrentPhase();
            if (currentPhase != null)
            {
                selectedBitmap = findImageByTag(currentPhase.piece);
                movePieceAnimation(currentPhase.to, currentPhase.from);
                bool success = maker.UndoMove();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            phase nextPhase = maker.GetNextPhase();
            if (nextPhase != null)
            {
                selectedBitmap = findImageByTag(nextPhase.piece);
                movePieceAnimation(nextPhase.from, nextPhase.to);
                bool success = maker.RedoMove();
            }
        }

        private void DirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isDragging && !isMoving)
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

                isMoving = true;
                
                //find selected image
                int ImageTag = maker.PieceOrder[chosenPiece.Item1, chosenPiece.Item2];
                selectedBitmap = findImageByTag(ImageTag);

                //move that image
                if (maker.MovePiece(chosenPiece, emptySpace))
                {
                    movePieceAnimation(chosenPiece, emptySpace);
                    
                    bool isWIn = maker.CheckWin();
                    if (isWIn)
                    {
                        _timer.Stop();
                        leftBottomCanvas.IsEnabled = false;
                        System.Windows.MessageBox.Show("You won!!!!");
                        resetVariables();
                    }
                }
            }
            
        }

        private void resetVariables()
        {
            currentImagePath = null;
            isGameStarted = false;
            leftBottomCanvas.Children.Clear();
            PreviewImage.Source = null;
            SaveGameButton.IsEnabled = false;
            StartGameButton.Content = "New Game";
            LoadGameButton.Content = "Load Game";
            LabelTimer.Content = null;
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
