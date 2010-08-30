using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace AssetImporter {

    public class Offset {
        public int X;
        public int Y;
    }

    public class OffsetEntry {
        public string Name;
        public Offset Offset;
    }

    public class Bmp {

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rgba[,] Data { get; private set; }

        public static Dictionary<string, Offset> Offsets = new Dictionary<string, Offset>();

        public static void SaveOffsets(string destinationDirectory) {
            var fileName = Path.Combine(destinationDirectory, "offsets.xml");
            var offsets = Offsets.Select(i => new OffsetEntry { Name = i.Key, Offset = i.Value }).ToArray();
            var serializer = new XmlSerializer(typeof(OffsetEntry[]));
            serializer.Serialize(File.Open(fileName, FileMode.Create), offsets);
        }

        public Bmp(int width, int height) {
            Width = width;
            Height = height;
            Data = new Rgba[height, width];
        }

        public void Save(string fileName, ImageFormat format) {
            var directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName)) {
                Directory.CreateDirectory(directoryName);
            }

            fileName += format == ImageFormat.Png ? ".png" : ".wtf";

            var bitmap = new Bitmap(Width, Height);
            for (int y = 0; y < Height; ++y) {
                for (int x = 0; x < Width; ++x) {
                    var color = System.Drawing.Color.FromArgb(Data[y, x].A, Data[y, x].R, Data[y, x].G, Data[y, x].B);
                    bitmap.SetPixel(x, y, color);
                }
            }
            bitmap.Save(fileName, format);
        }

        static public void SaveOffsets(string resourceName, int x, int y) {
            if (x != 0 || y != 0) {
                Bmp.Offsets[resourceName] = new Offset { X = x, Y = y };
            }

            
        }
    }
}
