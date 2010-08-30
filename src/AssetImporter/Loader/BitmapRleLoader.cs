using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace AssetImporter {

    public class BitmapRleLoader {

        public static ImageFormat ImageFormat = ImageFormat.Png;

        public void Load(string destinationDirectory, string resourceName, BinaryReader binaryReader) {

            short xOffset = binaryReader.ReadInt16();
            short yOffset = binaryReader.ReadInt16();
            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            ushort width = binaryReader.ReadUInt16();
            ushort height = binaryReader.ReadUInt16();
            binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
            uint length = binaryReader.ReadUInt32();
            var start = binaryReader.BaseStream.Position;

            if (length <= 0) {
                return;
            }

            binaryReader.BaseStream.Seek(2 * height, SeekOrigin.Current);
            var bmp = new Bmp(width, height);
            var colorData = bmp.Data;
            var palette = PaletteLoader.DefaultPalette;

            for (int y = 0; y < height; ++y) {
                int x = 0;
                while (x < width) {
                    // color pixels
                    byte count = binaryReader.ReadByte();
                    for (int i = 0; i < count; ++i) {
                        colorData[y, x] = palette.Colors[binaryReader.ReadByte()];
                        ++x;
                    }
                    // transparent pixels
                    count = binaryReader.ReadByte();
                    for (int i = 0; i < count; ++i) {
                        colorData[y, x] = new Rgba { A = 0 };
                        ++x;
                    }
                }
                // skip delimiter
                binaryReader.ReadByte();
            }
            // Make sure stream continues right after this format's data
            binaryReader.BaseStream.Seek(start + length, SeekOrigin.Begin);
            
            bmp.Save(Path.Combine(destinationDirectory, resourceName), ImageFormat);
            Bmp.SaveOffsets(resourceName, xOffset, yOffset);
        }
    }
}
