using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ThreeDMineTools.Tools;
using Image = System.Windows.Controls.Image;

namespace ThreeDMineTools.Components
{
    public class BlockTexture : UserControl
    {
        public static readonly DependencyProperty MainByteProperty =
            DependencyProperty.Register("MainByte", typeof((byte, byte)), typeof(BlockTexture), new PropertyMetadata(
                ((byte)0, (byte)0),
                new PropertyChangedCallback((o, args) =>
                {
                    if (args != null)
                        (o as BlockTexture).UpdateTexture(args.NewValue as (byte, byte)? ?? (0, 0));
                })));

        public (byte, byte) MainByte
        {
            get { return ((byte, byte))GetValue(MainByteProperty); }
            set { SetValue(MainByteProperty, value); }
        }

        private Image blockImage = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
        private TextBlock blockName = new() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)) };
        private TextBlock blockId = new() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)) };
        public BlockTexture()
        {
            Content = new Grid()
            {
                Children =
                {
                    blockImage,
                    blockName,
                    blockId
                },
                Width = 100,
                Height = 60
            };
        }

        public void UpdateTexture((byte, byte) val)
        {
            if (val.Item1 != 0 && BlockTextureProvider.Instance.ImageSources.ContainsKey(MainByte))
            {
                blockImage.Source = BlockTextureProvider.Instance.ImageSources[MainByte];
            }
            blockId.Text = val.ToString();
            blockName.Text = BlocksDatabase.Blocks[val];
        }
    }
}
