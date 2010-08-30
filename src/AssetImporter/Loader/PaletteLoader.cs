using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {

    public class PaletteLoader {

        public static List<Palette> Palettes = new List<Palette>();
        public static Palette DefaultPalette;

        public void Load(BinaryReader binaryReader) {

            binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
            var rgba = new Rgba[256];
            for (int i = 0; i < rgba.Length; ++i) {
                rgba[i] = new Rgba {
                    R = binaryReader.ReadByte(),
                    G = binaryReader.ReadByte(),
                    B = binaryReader.ReadByte()
                };
            }

            Palettes.Add(new Palette(rgba));
        }
    }
}
