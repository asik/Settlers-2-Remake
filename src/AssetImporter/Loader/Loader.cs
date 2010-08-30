using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AssetImporter.Loaders;

namespace AssetImporter {

    public enum BaseFormat : short {
        Unused1,
        Sound,
        BitmapRLE,
        Font,
        PlayerBitmap,
        Palette,
        Bob,
        ShadowBitmap,
        Map,
        Text,
        Raw,
        MapHeader,
        Ini,
        Unused2,
        RawBitmap
    }

    public class Loader {

        public static void LoadFile(string sourceFile, string destinationDirectory) {

            var extension = Path.GetExtension(sourceFile).ToUpper();
            switch (extension) {
                case ".ACT":
                    new ActLoader().Load(sourceFile);
                    break;
                case ".BBM":
                    new BbmLoader().Load(sourceFile);
                    break;
                case ".BOB":
                    new BobLoader().Load(sourceFile, CreateOwnDirectory(sourceFile, destinationDirectory));
                    break;
                case ".IDX":
                    new IdxLoader().Load(sourceFile, CreateOwnDirectory(sourceFile, destinationDirectory));
                    break;
                case ".LBM":
                    new LbmLoader().Load(sourceFile, destinationDirectory);
                    break;
                case ".LST":
                    new LstLoader().Load(sourceFile, CreateOwnDirectory(sourceFile, destinationDirectory));
                    break;
                case ".WLD":
                case ".SWD":
                    new WldLoader().Load(sourceFile, destinationDirectory);
                    break;
                case ".DAT":
                    new XMidiLoader().Load(sourceFile, destinationDirectory);
                    break;
                default:
                    throw new Exception("extension " + extension + " not supported");
            }

        }

        static string CreateOwnDirectory(string sourceFile, string destinationDirectory) {
            return Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFile));
        }

        public static void LoadBaseFormat(BaseFormat format, string destinationDirectory, string resourceName, BinaryReader binaryReader) {

            Directory.CreateDirectory(destinationDirectory);

            switch (format) {
                case BaseFormat.Font:
                    new FontLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                case BaseFormat.PlayerBitmap:
                    new PlayerBitmapLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                case BaseFormat.Palette:
                    new PaletteLoader().Load(binaryReader);
                    break;
                case BaseFormat.BitmapRLE:
                    new BitmapRleLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                case BaseFormat.RawBitmap:
                    new RawBitmapLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                case BaseFormat.ShadowBitmap:
                    new ShadowBitmapLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                case BaseFormat.Sound:
                    new SoundLoader().Load(destinationDirectory, resourceName, binaryReader);
                    break;
                default:
                    throw new Exception("Unsupported format : " + format.ToString());
            }
        }

        
    }
}
