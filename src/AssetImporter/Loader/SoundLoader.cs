using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter.Loaders {
    class SoundLoader {

        public SoundLoader() {
        }

        public void Load(string destinationDirectory, string resourceName, BinaryReader binaryReader) {

            var start = binaryReader.BaseStream.Position;
            uint totalLength = binaryReader.ReadUInt32();
            var container = new string(binaryReader.ReadChars(4));

            switch (container) {
                case "FORM":
                case "RIFF":
                    uint length = binaryReader.ReadUInt32(); // this is probably not really a length, with values like 0xcccccccc
                    var header = new string(binaryReader.ReadChars(4));
                    switch (header) {
                        case "XMID":
                        case "XDIR":
                            new XMidiLoader().Load(destinationDirectory, resourceName, binaryReader, totalLength);
                            break;
                        case "WAVE":
                            break;
                    }
                    break;
                default:
                    // Possibly a WAV but without a header
                    binaryReader.BaseStream.Seek(start, SeekOrigin.Begin);
                    new WaveLoader().Load(destinationDirectory, resourceName, binaryReader, (int)totalLength, true);
                    break;
            }
        }
    }
}
