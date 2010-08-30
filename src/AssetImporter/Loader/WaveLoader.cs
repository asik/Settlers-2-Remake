using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AssetImporter {
    public class WaveLoader {

        Dictionary<string, string> names = new Dictionary<string, string>() {
            { "SOUND1",     "1"                     },
            { "SOUND2",     "armory"                }, // 16000 hz
            { "SOUND3",     "axe on wood"           },
            { "SOUND4",     "sawing1"               },
            { "SOUND5",     "sawing2"               },
            { "SOUND6",     "construction1"         },
            { "SOUND7",     "plop"                  },
            { "SOUND8",     "iron smelter"          },
            { "SOUND9",     "hammering muddled"     },
            { "SOUND10",    "spilling"              },
            { "SOUND11",    "spinning"              }, // 9000 hz
            { "SOUND12",    "seeding"               },
            { "SOUND13",    "harvesting"            },
            { "SOUND14",    "low thud"              },
            { "SOUND15",    "15"                    },
            { "SOUND16",    "mechanism"             },
            { "SOUND17",    "click1"                },
            { "SOUND18",    "18"                    },
            { "SOUND19",    "construction2"         },
            { "SOUND20",    "soft rumbling"         },
            { "SOUND21",    "21"                    },
            { "SOUND22",    "creaking"              },
            { "SOUND23",    "23"                    },
            { "SOUND24",    "geologist searching"   },
            { "SOUND25",    "pulling water"         },
            { "SOUND26",    "rowing"                },
            { "SOUND27",    "timber"                }, // 9000 hz
            { "SOUND28",    "pigs1"                 },
            { "SOUND29",    "bird1"                 }, 
            { "SOUND30",    "bird2"                 }, 
            { "SOUND31",    "bird3"                 }, // clicks; needs at least a 50-sample fade-in 
            { "SOUND32",    "bird4"                 },
            { "SOUND33",    "bird5"                 },
            { "SOUND34",    "pigs2"                 },
            { "SOUND35",    "pigs3"                 },
            { "SOUND36",    "sheep"                 }, // 6000 hz
            { "SOUND37",    "duck"                  },
            { "SOUND38",    "burning"               },
            { "SOUND39",    "water1"                },
            { "SOUND40",    "water2"                },
            { "SOUND41",    "water3"                },
            { "SOUND42",    "sword fight1"          },
            { "SOUND43",    "sword fight2"          },
            { "SOUND44",    "sword fight3"          },
            { "SOUND45",    "death"                 }, // 6000 hz
            { "SOUND46",    "46"                    },
            { "SOUND47",    "yipee"                 }, // 9000 hz
            { "SOUND49",    "chord"                 },
            { "SOUND50",    "victory"               },
            { "SOUND51",    "clickUI"               },
            { "SOUND52",    "clickUI2"              },
            { "SOUND53",    "UI sound"              },
            { "SOUND54",    "message"               },
        };

        Dictionary<string, int> frequencyOverride = new Dictionary<string, int>() {
            { "SOUND2",     16000   }, 
            { "SOUND11",    9000    }, 
            { "SOUND36",    6000    }, 
            { "SOUND45",    6000    }, 
            { "SOUND47",    9000    }, 
        };

        class Wave {
            public byte[] FileTypeBlocID = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
            public uint FileSize = 44;
            public byte[] FormatID = { 0x57, 0x41, 0x56, 0x45 }; // "WAVE"

            public byte[] FormatBlocID = { 0x66, 0x6D, 0x74, 0x20 }; // "fmt "
            public uint BlocSize = 0x10;

            public ushort AudioFormat = 1;
            public ushort NumChannels = 1;
            public uint Frequency = 11025;
            public uint BytesPerSec = 11025;
            public ushort BytesPerBloc = 1;
            public ushort BitsPerSample = 8;

            public byte[] DataBlocID = { 0x64, 0x61, 0x74, 0x61 }; // "data"
            public uint DataSize = 0;
            public byte[] Data = { };

            public void Write(BinaryWriter binaryWriter) {
                binaryWriter.Write(FileTypeBlocID);
                binaryWriter.Write(FileSize);
                binaryWriter.Write(FormatID);
                binaryWriter.Write(FormatBlocID);
                binaryWriter.Write(BlocSize);
                binaryWriter.Write(AudioFormat);
                binaryWriter.Write(NumChannels);
                binaryWriter.Write(Frequency);
                binaryWriter.Write(BytesPerSec);
                binaryWriter.Write(BytesPerBloc);
                binaryWriter.Write(BitsPerSample);
                binaryWriter.Write(DataBlocID);
                binaryWriter.Write(DataSize);
                binaryWriter.Write(Data);
            }

            public const int HeaderSize = 44;
        }

        public void Load(string destinationDirectory, string resourceName, BinaryReader binaryReader, int totalLength, bool noHeader) {

            if (totalLength < Wave.HeaderSize) {
                binaryReader.BaseStream.Seek(totalLength, SeekOrigin.Current);
                return;
            }

            var bytesToSkip = 4;
            binaryReader.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);
            var wave = frequencyOverride.Keys.Contains(resourceName) ?
                new Wave { 
                    Frequency = (uint)frequencyOverride[resourceName], 
                    BytesPerSec = (uint)frequencyOverride[resourceName] 
                } :
                new Wave();
            wave.Data = binaryReader.ReadBytes(totalLength - bytesToSkip);
            wave.DataSize = (uint)wave.Data.Length;
            wave.FileSize = (uint)(wave.Data.Length + Wave.HeaderSize - 8);
            using (var binaryWriter = new BinaryWriter(File.Open(Path.Combine(destinationDirectory, names[resourceName]) + ".wav", FileMode.Create))) {
                wave.Write(binaryWriter);
            }
        }
    }
}
