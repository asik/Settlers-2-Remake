using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace AssetImporter {
    public class TextureLoader {

        // Tentative to decrypt the DATA/TEXTURE/GOU*.DAT files, which look like 256x256 paletted textures
        public void Load(string sourceFileName, string destinationDirectory) {
            using (var binaryReader = new BinaryReader(File.Open(sourceFileName, FileMode.Open))) {
                if (binaryReader.BaseStream.Length != 65536) {
                    throw new Exception(sourceFileName + " is an invalid texture");
                }

                var palette = PaletteLoader.DefaultPalette;
                var bmp = new Bmp(256, 256);
                var colorData = bmp.Data;

                for (int y = 0; y < 256; ++y) {
                    for (int x = 0; x < 256; ++x) {
                        colorData[y, x] = palette.Colors[binaryReader.ReadByte()];
                    }
                }

                Debug.Assert(binaryReader.BaseStream.Position == binaryReader.BaseStream.Length);
                var destinationFileName = Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFileName));
                bmp.Save(destinationFileName, ImageFormat.Png);
            }
        }

    }
}
