using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;

namespace AssetImporter {
    class RawBitmapLoader {

        public static ImageFormat ImageFormat = ImageFormat.Png;

        public void Load(string destinationDirectory, string resourceName, BinaryReader binaryReader) {
            binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
            uint length = binaryReader.ReadUInt32();
            byte[] imageData = binaryReader.ReadBytes((int)length);
            short xOffset = binaryReader.ReadInt16();
            short yOffset = binaryReader.ReadInt16();
            ushort width = binaryReader.ReadUInt16();
            ushort height = binaryReader.ReadUInt16();
            binaryReader.BaseStream.Seek(8, SeekOrigin.Current);

            var bmp = new Bmp(width, height);
            var colorData = bmp.Data;
            var palette = PaletteLoader.DefaultPalette;
            int index = 0;
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    colorData[y, x] = palette.Colors[imageData[index]];
                    ++index;
                }
            }

            bmp.Save(Path.Combine(destinationDirectory, resourceName), ImageFormat);
            Bmp.SaveOffsets(resourceName, xOffset, yOffset);
        }
    }
}
