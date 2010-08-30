using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;

namespace AssetImporter {
    public class LbmLoader {

        public void Load(string sourceFile, string destinationDirectory) {
            lbmName = sourceFile;
            using (var binaryReader = new BinaryReader(File.Open(sourceFile, FileMode.Open))) {
                var form = new string(binaryReader.ReadChars(4));
                if (form != "FORM") {
                    throw new Exception(sourceFile + " doesn't appear to be a valid LBM");
                }
                uint length = binaryReader.ReadUInt32();
                var pbm = new string(binaryReader.ReadChars(4));
                if (pbm != "PBM ") {
                    throw new Exception(sourceFile + " doesn't appear to be a valid LBM");
                }

                while (!readChunk(binaryReader)) {
                }

            }

            bmp.Save(Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFile)), ImageFormat.Png);
        }

        bool readChunk(BinaryReader binaryReader) {
            var id = new string(binaryReader.ReadChars(4));
            uint length = binaryReader.BE_ReadUInt32();
            if (length % 2 != 0) {
                ++length;
            }

            switch (id) {
                case "BMHD":
                    width = binaryReader.BE_ReadUInt16();
                    height = binaryReader.BE_ReadUInt16();
                    binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
                    colorDepth = binaryReader.ReadUInt16();
                    compression = binaryReader.ReadUInt16();
                    binaryReader.BaseStream.Seek(length - 12, SeekOrigin.Current); // Skip remaining bytes of this chunk

                    bmp = new Bmp(width, height);
                    return false;
                case "CMAP":
                    if (length != 768) {
                        throw new Exception("Invalid color palette in " + lbmName);
                    }
                    for (int i = 0; i < 256; ++i) {
                        palette[i] = new Rgba {
                            R = binaryReader.ReadByte(),
                            G = binaryReader.ReadByte(),
                            B = binaryReader.ReadByte(),
                            A = byte.MaxValue
                        };
                    }
                    return false;
                case "BODY":
                    if (compression != 1) {
                        throw new Exception("OMG WTF?!");
                    }
                    int numPixels = 0;
                    bool compressed = false;
                    byte compressedIndex = 0;

                    for (int y = 0; y < height; ++y) {
                        for (int x = 0; x < width; ++x) {

                            if (numPixels == 0) {
                                byte indicator = binaryReader.ReadByte();
                                compressed = indicator > 128;
                                if (compressed) {
                                    numPixels = 255 - indicator + 2;
                                    compressedIndex = binaryReader.ReadByte();
                                }
                                else {
                                    numPixels = indicator + 1;
                                }
                            }
                            var index = compressed ? compressedIndex : binaryReader.ReadByte();
                            bmp.Data[y, x] = palette[index];
                            --numPixels;
                        }
                    }
                    return true;
                default:
                    binaryReader.BaseStream.Seek(length, SeekOrigin.Current);
                    return false;
            }
        }

        Bmp bmp;        
        Rgba[] palette = new Rgba[256];
        ushort width;
        ushort height;
        ushort colorDepth;
        ushort compression;
        string lbmName;
    }
}
