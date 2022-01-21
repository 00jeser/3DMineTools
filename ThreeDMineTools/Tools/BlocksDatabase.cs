using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualBasic.FileIO;

namespace ThreeDMineTools.Tools
{
    public class BlocksDatabase
    {
        private static Dictionary<(byte, byte), string> Init()
        {
            var blocks = new Dictionary<(byte, byte), string>();

            using (TextFieldParser parser = new TextFieldParser(Assembly.GetExecutingAssembly().GetManifestResourceStream("ThreeDMineTools.Textures.blocksNames.csv")))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");
                List<string> names = new List<string>(parser.ReadFields());
                int langIndex = names.IndexOf(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag);
                if(langIndex == -1) langIndex = 3;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    var block = (byte.Parse(fields[0]), byte.Parse(fields[1]));
                    blocks[block] = fields[langIndex];
                }
            }

            return blocks;
        }
        public static Dictionary<(byte, byte), string> Blocks = Init();
    }
}
