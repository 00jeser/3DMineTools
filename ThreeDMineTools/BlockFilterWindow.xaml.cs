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

        private async void Window_OnLoaded(object sender, RoutedEventArgs e)
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

        private void selectAllWool(object sender, RoutedEventArgs e)
        {
            var ls = BlocksDatabase.Blocks.Keys.ToArray();
            list.SelectedItems.Add(ls[34]);
            list.SelectedItems.Add(ls[35]);
            list.SelectedItems.Add(ls[36]);
            list.SelectedItems.Add(ls[37]);
            list.SelectedItems.Add(ls[38]);
            list.SelectedItems.Add(ls[39]);
            list.SelectedItems.Add(ls[40]);
            list.SelectedItems.Add(ls[41]);
            list.SelectedItems.Add(ls[42]);
            list.SelectedItems.Add(ls[43]);
            list.SelectedItems.Add(ls[44]);
            list.SelectedItems.Add(ls[45]);
            list.SelectedItems.Add(ls[46]);
            list.SelectedItems.Add(ls[47]);
            list.SelectedItems.Add(ls[48]);
            list.SelectedItems.Add(ls[49]);
            list.Focus();
        }

        private void selectAll(object sender, RoutedEventArgs e)
        {
            list.SelectAll();
            list.Focus();
        }

        private void deselectAll(object sender, RoutedEventArgs e)
        {
            list.SelectedItems.Clear();
            list.Focus();
        }

        private void selectAllCeramic(object sender, RoutedEventArgs e)
        {
            var ls = BlocksDatabase.Blocks.Keys.ToArray();
            list.SelectedItems.Add(ls[79]);
            list.SelectedItems.Add(ls[80]);
            list.SelectedItems.Add(ls[81]);
            list.SelectedItems.Add(ls[82]);
            list.SelectedItems.Add(ls[83]);
            list.SelectedItems.Add(ls[84]);
            list.SelectedItems.Add(ls[85]);
            list.SelectedItems.Add(ls[86]);
            list.SelectedItems.Add(ls[87]);
            list.SelectedItems.Add(ls[88]);
            list.SelectedItems.Add(ls[89]);
            list.SelectedItems.Add(ls[90]);
            list.SelectedItems.Add(ls[91]);
            list.SelectedItems.Add(ls[92]);
            list.SelectedItems.Add(ls[93]);
            list.SelectedItems.Add(ls[94]);
            list.Focus();

        }

        private void selectAllConcrete(object sender, RoutedEventArgs e)
        {
            var ls = BlocksDatabase.Blocks.Keys.ToArray();
            list.SelectedItems.Add(ls[109]);
            list.SelectedItems.Add(ls[110]);
            list.SelectedItems.Add(ls[111]);
            list.SelectedItems.Add(ls[112]);
            list.SelectedItems.Add(ls[113]);
            list.SelectedItems.Add(ls[114]);
            list.SelectedItems.Add(ls[115]);
            list.SelectedItems.Add(ls[116]);
            list.SelectedItems.Add(ls[117]);
            list.SelectedItems.Add(ls[118]);
            list.SelectedItems.Add(ls[119]);
            list.SelectedItems.Add(ls[120]);
            list.SelectedItems.Add(ls[121]);
            list.SelectedItems.Add(ls[122]);
            list.SelectedItems.Add(ls[123]);
            list.SelectedItems.Add(ls[124]);
            list.Focus();
        }
    }
}
