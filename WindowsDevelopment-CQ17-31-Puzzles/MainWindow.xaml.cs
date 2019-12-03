using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            drawUI();
        }

        private void drawUI()
        {
            // draw rows
            for (int i = 0; i <= rows; i++)
            {
                Line line_horizontal = new Line();
                line_horizontal.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineColor));
                line_horizontal.StrokeThickness = lineWeight;
                leftCanvas.Children.Add(line_horizontal);
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
                leftCanvas.Children.Add(line_vertical);
                line_vertical.X1 = startX + colWidth * j;
                line_vertical.Y1 = startY;
                line_vertical.X2 = startX + colWidth * j;
                line_vertical.Y2 = startY + rows * rowHeight;
            }
        }
    }
}
