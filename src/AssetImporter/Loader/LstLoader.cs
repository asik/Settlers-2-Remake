using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class LstLoader {
        public void Load(string sourceFileName, string destinationDirectory) {

            // This might not even be a real LST although it has .LST extension. WTF?
            // So, we have to do this in two steps.
            ushort header;
            using (var binaryReader = new BinaryReader(File.Open(sourceFileName, FileMode.Open))) {
                header = binaryReader.BE_ReadUInt16();
            }
            // If it turns out that this is a txt file, use the appropriate loader for that
            // (Does that actually ever happen? Would be cool to ditch all this verification shit)
            if (header == 0xE7FD) {
                new TxtLoader().Load(sourceFileName, destinationDirectory);
                return;
            }
            // Now we're sure it's a LST... or are we?
            if (header != 0x204E) {
                throw new Exception("Invalid LST file : " + sourceFileName);
            }
            // Ok now we're REALLY sure! Phew.
            using (var binaryReader = new BinaryReader(File.Open(sourceFileName, FileMode.Open))) {
                binaryReader.BE_ReadUInt16(); // Skip header, we've just read it
                binaryReader.ReadUInt32(); // Skip number of entries, we'll just read until end of file anyway
                int numEntry = 0;
                string baseName = Path.GetFileNameWithoutExtension(sourceFileName);
                while(binaryReader.BaseStream.Position + 1 < binaryReader.BaseStream.Length) {                    
                    if ( binaryReader.BE_ReadInt16() != 0x0100) {
                        continue;
                    }
                    var baseFormat = (BaseFormat)binaryReader.ReadInt16();
                    Loader.LoadBaseFormat(baseFormat, destinationDirectory, baseName + numEntry++, binaryReader);
                }
            }
        }
    }
}
