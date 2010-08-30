using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace AssetImporter {
    public class PlayerBitmapLoader {

        short xOffset;
        short yOffset;
        ushort width;
        ushort height;

        public static ImageFormat ImageFormat = ImageFormat.Png;

        // As found in .DAT archives (RESOURCE.DAT, IO.DAT)
        public void Load(string destinationDirectory, string baseName, BinaryReader binaryReader) {
            xOffset = binaryReader.ReadInt16();
            yOffset = binaryReader.ReadInt16();
            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            width = binaryReader.ReadUInt16();
            height = binaryReader.ReadUInt16();
            binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
            uint length = binaryReader.ReadUInt32();

            var imageOffset = binaryReader.BaseStream.Position;

            var rowDataOffsets = new ushort[height];
            for (int i = 0; i < height; ++i) {
                rowDataOffsets[i] = binaryReader.ReadUInt16();
            }

            var bmp = new Bmp(width, height);
            var imageData = binaryReader.ReadBytes((int)length - 2*height);

            ReadImage(imageData, rowDataOffsets, false, bmp.Data);

            Debug.Assert(binaryReader.BaseStream.Position - imageOffset == length);
            bmp.Save(Path.Combine(destinationDirectory, baseName), ImageFormat);
            Bmp.SaveOffsets(baseName, xOffset, yOffset);
        }
        
        // As found in .BOB archives
        public void Load(byte[] imageData, string destinationDirectory, string baseName, BinaryReader binaryReader) {

            ushort id = binaryReader.BE_ReadUInt16();
            if (id != 0xF401) {
                throw new Exception(baseName + " is an invalid PlayerBitmap");
            }

            width = 32;
            height = binaryReader.ReadByte();
            var rowDataOffsets = new ushort[height];
            for (int i = 0; i < height; ++i) {
                rowDataOffsets[i] = binaryReader.ReadUInt16();
            }
            xOffset = 16;
            yOffset = binaryReader.ReadByte();

            var bmp = new Bmp(width, height);
            ReadImage(imageData, rowDataOffsets, true, bmp.Data);

            bmp.Save(Path.Combine(destinationDirectory, baseName), ImageFormat);
            Bmp.SaveOffsets(baseName, xOffset, yOffset);
        }

        // Used for loading .BOB archives. Quite a horrible method signature if you ask me!
        public void Load(ushort width, ushort height, short xOffset, short yOffset, byte[] imageData, ushort[] rowDataOffsets, string destinationDirectory, string baseName) {
            this.width = width;
            this.height = height;
            this.xOffset = xOffset;
            this.yOffset = yOffset;

            var bmp = new Bmp(width, height);
            ReadImage(imageData, rowDataOffsets, true, bmp.Data);

            bmp.Save(Path.Combine(destinationDirectory, baseName), ImageFormat);
            Bmp.SaveOffsets(baseName, xOffset, yOffset);
        }


        void ReadImage(byte[] imageData, ushort[] rowDataOffsets, bool absoluteOffsets, Rgba[,] colorData) {

            var palette = PaletteLoader.DefaultPalette;
            int position = 0;

            for (int y = 0; y < height; ++y) {
                position = rowDataOffsets[y] - (absoluteOffsets ? 0 : 2 * height);

                ushort x = 0;

                while (x < width) {
                    byte shift = imageData[position++];
                    if (shift < 0x40) {
                        for (byte i = 0; i < shift; ++i) {
                            // Set to transparent
                            colorData[y, x].A = 0;
                            ++x;
                        }
                    }
                    else if (shift < 0x80) {
                        shift -= 0x40;
                        for (byte i = 0; i < shift; ++i) {
                            // Set to corresponding color in palette
                            var color = palette.Colors[imageData[position++]];
                            colorData[y, x] = color;
                            ++x;
                        }
                    }
                    else if (shift < 0xC0) {
                        shift -= 0x80;
                        byte transparency = imageData[position++]; // libsiedler2 uses this for tex_pdata. I don't know what purpose does that serve.
                        for (byte i = 0; i < shift; ++i) {
                            // TEST: hardcode a visible color here
                            colorData[y, x] = new Rgba { A = (byte)(byte.MaxValue - 64 * transparency), R = 255, B = 0, G = 0 };
                            ++x;
                        }
                    }
                    else {
                        shift -= 0xC0;
                        var color = palette.Colors[imageData[position++]];
                        for (byte i = 0; i < shift; ++i) {
                            colorData[y, x] = color;
                            ++x;
                        }
                    }
                }
            }
        }
    }
}
