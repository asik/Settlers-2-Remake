using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class WldLoader {

        // I don't think it is worth it to convert .WLDs to something else.
        // Mainly because RTTR uses that format (.SWD is exactly the same format) and I want my maps to be compatible with RTTR.
        // Also because, what to convert to? A textual, human-readable format? It wouldn't be that readable, and a good
        // map editor negates the need for that pretty much.
        // I thought about doing a "cleaned up", "WORLD V2.0" format (because "WORLD V1.0" has a lot of unknown content), but really
        // is it so important to save a few KBs that it warrants sacrificing compatibility with RTTR? The answer is no. Or at least, not
        // before I get to write the map editor.

        // I change the extension to .SWD just to make it clear that this is a "converted" format. It is only an implementation
        // detail that I use the "WORLD V1.0" format: I could very well use another in the future.

        public void Load(string sourceFileName, string destinationDirectory) {

            var originalFileName = Path.GetFileNameWithoutExtension(sourceFileName);
            var destinationFileName = Path.Combine(destinationDirectory, originalFileName + ".SWD");
            Directory.CreateDirectory(destinationDirectory);

            File.Copy(sourceFileName, destinationFileName, true);
        }
    }
}
