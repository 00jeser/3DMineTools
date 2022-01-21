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
using ThreeDMineTools.Tools;

namespace ThreeDMineTools
{
    /// <summary>
    /// Логика взаимодействия для BlockFilterWindow.xaml
    /// </summary>
    public partial class BlockFilterWindow : Window
    {
        public List<(byte, byte)> selectedBytes;
        public BlockFilterWindow()
        {
            InitializeComponent();
        }

        private void Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            list.ItemsSource = BlocksDatabase.Blocks.Keys;
            list.SelectAll();
            list.Focus();
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            selectedBytes = list.SelectedItems.Cast<(byte, byte)>().ToList();
            Close();
        }
    }
}
