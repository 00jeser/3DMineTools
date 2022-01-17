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

namespace ThreeDMineTools
{
    public partial class ApplicationWindow : Window
    {
        public ApplicationWindow()
        {
            InitializeComponent();
        }

        private void CreateVoxelPage(object sender, RoutedEventArgs e)
        {
            var clsBtn = new Button()
            {
                Content = new TextBlock()
                {
                    Text = "X",
                    FontFamily = new FontFamily("Consoles")
                },
                BorderBrush = new SolidColorBrush(Color.FromArgb(0,0,0,0)),
                Background = new SolidColorBrush(Color.FromArgb(0,0,0,0)),
                Margin = new Thickness(10,0,0,0)
            };
            var page = new TabItem()
            {
                Header = new StackPanel()
                {
                    Children =
                    {
                        new TextBlock() {Text = "Model to schematic"},
                        clsBtn
                    },
                    Orientation = Orientation.Horizontal
                },
                Content = new VoxelPage()
            };
            clsBtn.Click += (_, _) =>
            {
                Tabs.Items.Remove(page);
                (page.Content as VoxelPage).Dispose();
            };
            Tabs.Items.Add(page);
            Tabs.SelectedIndex = Tabs.Items.Count - 1;
        }
    }
}
