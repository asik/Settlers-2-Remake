using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AssetImporter;

namespace AssetImporter {
    public class IdxLoader {

        struct IdxEntry {
            public string Name;
            public BaseFormat BaseFormat;
            public uint Offset;
        }

        List<IdxEntry> Entries = new List<IdxEntry>();

        public void Load(string sourceFileName, string destinationDirectory) {

            var idxFileName = sourceFileName;
            var datFileName = sourceFileName.Substring(0, sourceFileName.Length - 3) + "DAT";

            using (var idxFile = new BinaryReader(File.Open(idxFileName, FileMode.Open)))
            using (var datFile = new BinaryReader(File.Open(datFileName, FileMode.Open))) {
                var numEntries = idxFile.ReadUInt32();
                for (var i = 0; i < numEntries; ++i) {
                    var name = idxFile.ReadNullTerminatedASCIIString(16);
                    var offset = idxFile.ReadUInt32();
                    idxFile.BaseStream.Seek(6, SeekOrigin.Current);
                    datFile.BaseStream.Seek(offset, SeekOrigin.Begin);

                    var idxBaseFormat = (BaseFormat)idxFile.ReadInt16();

                    Entries.Add(new IdxEntry { Name = name, BaseFormat = idxBaseFormat, Offset = offset });
                }

                foreach (var entry in Entries) {
                    datFile.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                    var datBaseFormat = (BaseFormat)datFile.ReadInt16();

                    if (entry.BaseFormat != datBaseFormat) {
                        throw new Exception("WTF");
                    }
                    Loader.LoadBaseFormat(entry.BaseFormat, destinationDirectory, entry.Name, datFile);
                }
            }
        }
    }
}
