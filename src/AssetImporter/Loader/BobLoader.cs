using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class BobLoader {
        public void Load(string sourceFile, string destinationDirectory) {

            using (var binaryReader = new BinaryReader(File.Open(sourceFile, FileMode.Open))) {

                uint header = binaryReader.BE_ReadUInt32();
                if (header != 0xF601F501) {
                    throw new Exception(sourceFile + "is an invalid .BOB file");
                }

                // Size of the first color block
                ushort size = binaryReader.ReadUInt16();

                // Read first color block
                var imageData = binaryReader.ReadBytes(size);

                var playerBitmapLoader = new PlayerBitmapLoader();
                for (int i = 0; i < 96; ++i) {
                    playerBitmapLoader.Load(imageData, destinationDirectory, "untitled" + i, binaryReader);
                }

                // 6 next color blocks
                var color = new byte[6][];
                var sizes = new ushort[6];

                for (int i = 0; i < 6; ++i) {
                    ushort id = binaryReader.BE_ReadUInt16();
                    if (id != 0xF501) {
                        throw new Exception(sourceFile + "is an invalid .BOB file");
                    }
                    sizes[i] = binaryReader.ReadUInt16();
                    color[i] = binaryReader.ReadBytes(sizes[i]);
                }

                ushort numImagesProduced = binaryReader.ReadUInt16();

                // Read heights, row data offsets and yOffsets for each image to be produced
                var heights = new byte[numImagesProduced];
                var rowDataOffsets = new ushort[numImagesProduced][];
                var yOffsets = new byte[numImagesProduced];

                for (int i = 0; i < numImagesProduced; ++i) {
                    ushort id = binaryReader.BE_ReadUInt16();
                    if (id != 0xF401) {
                        throw new Exception(sourceFile + "is an invalid .BOB file");
                    }

                    heights[i] = binaryReader.ReadByte();
                    rowDataOffsets[i] = new ushort[heights[i]];
                    for (int j = 0; j < heights[i]; ++j) {
                        rowDataOffsets[i][j] = binaryReader.ReadUInt16();
                    }

                    yOffsets[i] = binaryReader.ReadByte();
                }

                ushort itemCount = binaryReader.ReadUInt16();
                var used = new bool[numImagesProduced];

                // This algorithm is a bit hard to understand. Why not just loop from 0 to numImagesProduced?
                // The point is that the file specifies what to produce in a certain order, and certain images
                // are actually repeated.
                var resourceName = Path.GetFileNameWithoutExtension(sourceFile);
                for (int i = 0; i < itemCount; ++i) {
                    ushort index = binaryReader.ReadUInt16();
                    if (!used[index]) {
                        playerBitmapLoader.Load(
                            32, 
                            heights[index], 
                            16,
                            yOffsets[index], 
                            color[i % 6], 
                            rowDataOffsets[index], 
                            destinationDirectory,
                            resourceName + i
                        );
                    }
                    used[index] = true;
                    binaryReader.BaseStream.Seek(2, SeekOrigin.Current);
                }
            }
        }
    }
}
