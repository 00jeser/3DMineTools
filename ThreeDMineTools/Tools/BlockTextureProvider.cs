using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThreeDMineTools.Tools
{
    public class BlockTextureProvider
    {
        private static BlockTextureProvider _instance;

        public static BlockTextureProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BlockTextureProvider();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public Dictionary<(byte, byte), ImageSource> ImageSources { get; set; } = new();
        public BlockTextureProvider()
        {
            foreach (var block in BlocksDatabase.Blocks.Keys)
            {
                string filePath =
                    $"{Path.GetDirectoryName(Environment.ProcessPath)}\\Textures\\{block.Item1}_{block.Item2}.png";
                if (File.Exists(filePath))
                    ImageSources[block] = new BitmapImage(new Uri(filePath));
            }
        }
    }
}
