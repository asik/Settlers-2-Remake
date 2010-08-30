using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AssetImporter {
    public class Font {
        public byte XSpacing;
        public byte YSpacing;

        [XmlIgnore]
        public Bmp[] Characters = new Bmp[256 - 32];
        public string[] CharacterFileNames = new string[256 - 32];
    }
}
