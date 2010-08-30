using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetImporter {
    public class Palette {
        public Rgba[] Colors = new Rgba[256];

        public Palette(Rgba[] rgba) {
            Colors = rgba;
        }
    }
}
