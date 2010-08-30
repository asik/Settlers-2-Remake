using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;

namespace AssetImporter {
    public class ShadowBitmapLoader {

        public void Load(string destinationDirectory, string resourceName, BinaryReader binaryReader) {

            short xOffset = binaryReader.ReadInt16();
            short yOffset = binaryReader.ReadInt16();
            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            ushort width = binaryReader.ReadUInt16();
            ushort height = binaryReader.ReadUInt16();
            binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
            uint length = binaryReader.ReadUInt32();
            var start = binaryReader.BaseStream.Position;

            // Set to magenta for now (should be white ("gray" as RTTR calls it))
            var gray = new Rgba { 
                R = byte.MaxValue, 
                G = 0,//byte.MaxValue, 
                B = byte.MaxValue, 
                A = byte.MaxValue 
            };
            var transparent = new Rgba { 
                R = byte.MaxValue, 
                G = byte.MaxValue, 
                B = byte.MaxValue, 
                A = 0 
            };

            // Skip row indexes, not necessary
            binaryReader.BaseStream.Seek(2 * height, SeekOrigin.Current);
            var bmp = new Bmp(width, height);
            var palette = PaletteLoader.DefaultPalette;

            for (int y = 0; y < height; ++y) {
                int x = 0;
                while (x < width) {
                    byte count = binaryReader.ReadByte();
                    for (int i = 0; i < count; ++i) {
                        bmp.Data[y, x] = gray;
                        ++x;
                    }
                    count = binaryReader.ReadByte();
                    for (int i = 0; i < count; ++i) {
                        bmp.Data[y, x] = transparent;
                        ++x;
                    }
                }
                // Skip delimiter
                binaryReader.ReadByte();
            }

            binaryReader.BaseStream.Seek(start + length, SeekOrigin.Begin);
            bmp.Save(Path.Combine(destinationDirectory, resourceName), ImageFormat.Png);
            Bmp.SaveOffsets(resourceName, xOffset, yOffset);
        }
    }
}
