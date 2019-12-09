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
using System.Windows.Shapes;

namespace WindowsDevelopment_CQ17_31_Puzzles
{
    /// <summary>
    /// Interaction logic for DifficultyDialog.xaml
    /// </summary>
    public partial class DifficultyDialog : Window
    {
        public int Difficulty;
        public DifficultyDialog()
        {
            InitializeComponent();
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            Difficulty = PuzzleMaker.Medium;
            if (RadioButton1.IsChecked == true)
            {
                Difficulty = PuzzleMaker.Easy;
            }

            if (RadioButton2.IsChecked == true)
            {
                Difficulty = PuzzleMaker.Medium;
            }

            if (RadioButton1.IsChecked == true)
            {
                Difficulty = PuzzleMaker.Hard;
            }
            DialogResult = true;
        }
    }
}
