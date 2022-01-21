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
    public partial class ColorForBlockSelectorWindow : Window
    {
        public Dictionary<Color, (byte, byte)> Colors = new Dictionary<Color, (byte, byte)>();
        public Dictionary<(byte, byte), Color> Blocks
        {
            get
            {
                var rez = new Dictionary<(byte, byte), Color>();
                foreach (var tuple in Colors)
                {
                    rez[tuple.Value] = tuple.Key;
                }
                return rez;
            }
        }

        private List<Color> ColorsList = new();
        public ColorForBlockSelectorWindow(List<Color> colors)
        {
            ColorsList = colors;
            InitializeComponent();
        }

        private void BlocksList_OnInitialized(object? sender, EventArgs e)
        {
            (sender as ListBox).ItemsSource = BlocksDatabase.Blocks.Keys;
            (sender as ListBox).SelectedIndex = 0;
        }

        private void ChangeColor(object sender, SelectionChangedEventArgs e)
        {
            Colors[(Color) (sender as ListBox).Tag] = ((byte, byte)) (sender as ListBox).SelectedItem;
        }

        private async void Window_OnInitialized(object? sender, EventArgs e)
        {
            await Task.Delay(1);
            list.ItemsSource = ColorsList;
        }
    }
}
