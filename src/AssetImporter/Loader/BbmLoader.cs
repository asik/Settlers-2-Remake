using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class BbmLoader {

        public BbmLoader() {
        }

        public void Load(string fileName) {
            using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open))) {
                var form = new string(binaryReader.ReadChars(4));
                if (form != "FORM") {
                    throw new InvalidDataException();
                }
                uint fileLength = binaryReader.ReadUInt32();
                var PBM_ = new string(binaryReader.ReadChars(4));
                if (PBM_ != "PBM ") {
                    throw new InvalidDataException();
                }
                while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length) {
                    var id = new string(binaryReader.ReadChars(4));
                    uint chunkLength = binaryReader.BE_ReadUInt32();
                    if (chunkLength % 2 != 0) {
                        ++chunkLength;
                    }
                    byte[] chunk = binaryReader.ReadBytes((int)chunkLength);
                    
                    if (id == "CMAP" && chunkLength == 256 * 3) {
                        var rgba = new Rgba[256];
                        for (int i = 0; i < 256; ++i) {
                            rgba[i] = new Rgba {
                                R = chunk[3 * i],
                                G = chunk[3 * i + 1],
                                B = chunk[3 * i + 2],
                                A = byte.MaxValue
                            };
                        }
                        var newPalette = new Palette(rgba);
                        PaletteLoader.Palettes.Add(newPalette);
                        if (Path.GetFileNameWithoutExtension(fileName) == "PAL5") {
                            PaletteLoader.DefaultPalette = newPalette;
                        }
                    }
                }
            }
        }


    }
}
