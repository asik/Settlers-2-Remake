using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Drawing.Imaging;

namespace AssetImporter {
    public class FontLoader {

        public void Load(string destinationDirectory, string fontName, BinaryReader binaryReader) {

            var destination = Path.Combine(destinationDirectory, fontName);
            Directory.CreateDirectory(destination);

            var font = new Font();
            font.XSpacing = binaryReader.ReadByte();
            font.YSpacing = binaryReader.ReadByte();

            for (int i = 0; i < font.Characters.Length; ++i) {
                var baseFormat = (BaseFormat)binaryReader.ReadInt16();
                if (baseFormat == BaseFormat.Unused1) {
                    continue;
                }

                var fileName = fontName + "_" + i;
                Loader.LoadBaseFormat(baseFormat, destination, fileName, binaryReader);
                font.CharacterFileNames[i] = fileName;
            }

            (new XmlSerializer(typeof(Font)))
                .Serialize(File.Open(Path.Combine(destination, fontName + ".xml"), FileMode.Create), font);
        }
    }
}
