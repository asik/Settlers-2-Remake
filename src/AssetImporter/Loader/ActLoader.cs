using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class ActLoader {

        public void Load(string sourceFileName) {
            if (new FileInfo(sourceFileName).Length != 768) {
                throw new InvalidDataException();
            }
            var colors = new Rgba[256];
            using (var binaryReader = new BinaryReader(File.Open(sourceFileName, FileMode.Open))) {
                for (int i = 0; i < 256; ++i) {
                    colors[i] = new Rgba {
                        R = binaryReader.ReadByte(),
                        G = binaryReader.ReadByte(),
                        B = binaryReader.ReadByte(),
                        A = byte.MaxValue
                    };
                }
            }

            PaletteLoader.Palettes.Add(new Palette(colors));
        }
    }
}
