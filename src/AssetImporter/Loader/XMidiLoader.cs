using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;



namespace AssetImporter {
    /// <summary>
    /// This class essentially reads xmidi data from a binary stream and outputs a .mid file.
    /// </summary>
    /// <remarks>
    /// The xmi-to-midi algorithm is translated almost literally from Peter "Corsix" Cawley's C++ implementation,
    /// which, at the time of this writing, can be found at http://code.google.com/p/corsix-th/
    /// </remarks>
    public class XMidiLoader {

        uint length;
        BinaryReader binaryReader;

        class MidiToken {
            public int Time;
            public byte[] Buffer;
            public byte Type;
            public byte Data;
            public int OriginalIndex; // to allow stable sorting
        }

        public void Load(string destinationDirectory, string resourceName, BinaryReader _binaryReader, uint _length) {
            binaryReader = _binaryReader;
            length = _length;
            var start = binaryReader.BaseStream.Position;

            SkipToEVNT();
            binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
            int tempo;
            List<MidiToken> tokenList = ReadTokens(out tempo);

            Directory.CreateDirectory(destinationDirectory);
            var targetFileName = Path.Combine(destinationDirectory, resourceName) + ".mid";
            using (var binaryWriter = new BinaryWriter(File.Open(targetFileName, FileMode.Create))) {
                WriteMidi(tempo, tokenList, binaryWriter);
            }
            binaryReader.BaseStream.Seek(start + length, SeekOrigin.Begin);
            ConvertToWav(targetFileName);
        }

        void ConvertToWav(string targetFileName) {
            var process = getTimidityProcess();
            if (!File.Exists(process.StartInfo.FileName)) {
                throw new Exception("Could not find midi-to-wav converter timidity++ in working directory");
            }
            process.StartInfo.Arguments += " -Ow \"" + targetFileName + "\"";

            //Converter.worker.ReportProgress(0, Environment.NewLine);
            ExecuteEmbeddedProcess(process);
            File.Delete(targetFileName);
        }
        
        
        void ExecuteEmbeddedProcess(Process process) {
            process.Start();
            //process.BeginErrorReadLine();
            //process.BeginOutputReadLine();
            process.WaitForExit();
        }
        
        
        
        Process getTimidityProcess() {	
            string fileName = "";
            string arguments = "";
            var process = new Process();
            if (Environment.OSVersion.Platform.ToString().StartsWith("Win")) {
                fileName = "timidity.exe";
            }
            else {
                fileName = "timidity";
                arguments = "-x " + "\"soundfont TimGM6mb.sf2\"";
            }

            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            //process.ErrorDataReceived += timidity_DataReceived;
            //process.OutputDataReceived += timidity_DataReceived;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            return process;
        }


        
        void timidity_DataReceived(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                Converter.worker.ReportProgress(0, "\t" + e.Data + Environment.NewLine );
            }
        }

        
        
        void SkipToEVNT() {
            var stringBuilder = new StringBuilder(5);
            while (stringBuilder.ToString() != "EVNT") {
                stringBuilder.Append(Encoding.ASCII.GetString(binaryReader.ReadBytes(1)));
                if (stringBuilder.Length > 4) {
                    stringBuilder.Remove(0, 1);
                }
            }
        }

        
        
        List<MidiToken> ReadTokens(out int tempo) {
            var tokenList = new List<MidiToken>();
            bool end = false;
            bool tempoSet = false;
            tempo = 500000;
            MidiToken token;
            byte tokenType;
            int tokenTime = 0;

            while (!end) {                

                while (true) {
                    tokenType = binaryReader.ReadByte();

                    if ((tokenType & 0x80) != 0) {
                        break;
                    }
                    else {
                        tokenTime += 3 * tokenType;
                    }
                }
                token = NewToken(tokenList, tokenTime, tokenType);
                binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                token.Buffer = binaryReader.ReadBytes(1);
                binaryReader.BaseStream.Seek(-2, SeekOrigin.Current);

                switch (tokenType & 0xF0) {
                    case 0xC0:
                    case 0xD0:
                        token.Data = binaryReader.ReadByte();
                        token.Buffer = null;
                        break;
                    case 0x80:
                    case 0xA0:
                    case 0xB0:
                    case 0xE0:
                        token.Data = binaryReader.ReadByte();
                        binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                        break;
                    case 0x90: {
                            byte extendedType = binaryReader.ReadByte();
                            token.Data = extendedType;
                            binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                            token = NewToken(tokenList, tokenTime + (int)binaryReader.ReadUInt32_VariableLength() * 3, tokenType);
                            token.Data = extendedType;
                            token.Buffer = new byte[] { 0x00 };
                        }
                        break;
                    case 0xF0: {
                            byte extendedType = 0;
                            if (token.Type == 0xFF) {
                                extendedType = binaryReader.ReadByte();
                                if (extendedType == 0x2F) {
                                    end = true;
                                }
                                else if (extendedType == 0x51) {
                                    if (!tempoSet) {
                                        binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                                        tempo = (int)binaryReader.BE_ReadUInt24() * 3;
                                        tempoSet = true;
                                        binaryReader.BaseStream.Seek(-4, SeekOrigin.Current);
                                    }
                                    else {
                                        tokenList.RemoveAt(tokenList.Count - 1);
                                        uint toSkip = binaryReader.ReadUInt32_VariableLength();
                                        binaryReader.BaseStream.Seek(toSkip, SeekOrigin.Current);
                                        break;
                                    }
                                }
                            }
                            token.Data = extendedType;
                            uint bufferLength = binaryReader.ReadUInt32_VariableLength();
                            token.Buffer = binaryReader.ReadBytes((int)bufferLength);
                            break;
                        }
                }
            }
            return tokenList;
        }

        
        
        MidiToken NewToken(List<MidiToken> tokens, int time, byte type) {
            var token = new MidiToken { Time = time, Type = type, OriginalIndex = tokens.Count };
            tokens.Add(token);
            return token;
        }


        
        void WriteMidi(int tempo, List<MidiToken> tokenList, BinaryWriter binaryWriter) {
            binaryWriter.Write(Encoding.ASCII.GetBytes("MThd\0\0\0\x06\0\0\0\x01"));
            binaryWriter.BE_Write((ushort)((tempo * 3) / 25000));
            binaryWriter.Write(Encoding.ASCII.GetBytes("MTrk"), 0, 4);
            binaryWriter.Write(new byte[] { 0xde, 0xad, 0xbe, 0xef }); // dead beef OH YEAH!!! we'll eventually write the size there but we don't know it yet

            tokenList.Sort((t1, t2) => {
                // Stable sort : if two items have the same value, preserve original order
                // xmi2mid.cpp uses std::sort which is not guaranteed to be stable, but it just happens that it did preserve order in that particular case,
                // and the algorithm was relying on that (weird).
                // I could not assume stable sorting would occur so I had to force it, which is in fact the better thing to do.
                if (t1.Time == t2.Time) {
                    return t1.OriginalIndex.CompareTo(t2.OriginalIndex);
                }
                else {
                    return t1.Time.CompareTo(t2.Time);
                }
            });

            bool end = false;
            int tokenTime = 0;
            byte tokenType = 0;
            foreach (var token in tokenList) {
                if (end) {
                    break;
                }

                binaryWriter.WriteUInt32_VariableLength((uint)(token.Time - tokenTime));
                tokenTime = token.Time;
                if (token.Type >= 0xF0) {
                    binaryWriter.Write(token.Type);
                    tokenType = token.Type;
                    if (token.Type == 0xFF) {
                        binaryWriter.Write(token.Data);
                        end = token.Data == 0x2F;
                    }
                    binaryWriter.WriteUInt32_VariableLength((uint)token.Buffer.Length);
                    binaryWriter.Write(token.Buffer, 0, token.Buffer.Length);
                }
                else {
                    if (token.Type != tokenType) {
                        binaryWriter.Write(token.Type);
                        tokenType = token.Type;
                    }
                    binaryWriter.Write(token.Data);
                    if (token.Buffer != null) {
                        binaryWriter.Write(token.Buffer, 0, token.Buffer.Length);
                    }
                }
            }

            uint length = (uint)binaryWriter.BaseStream.Position - 22;
            binaryWriter.BaseStream.Seek(18, SeekOrigin.Begin);
            binaryWriter.BE_Write(length);
        }


        
        public void Load(string sourceFile, string destinationDirectory) {
            using (var binaryReader = new BinaryReader(File.Open(sourceFile, FileMode.Open))) {
                var container = new string(binaryReader.ReadChars(4));
                if (container != "FORM") {
                    throw new InvalidDataException(sourceFile + " is not a valid XMidi file");
                }
                binaryReader.BaseStream.Seek(4, SeekOrigin.Current);
                var format = new string(binaryReader.ReadChars(4));
                if (format != "XDIR") {
                    throw new InvalidDataException(sourceFile + " is not a valid XMidi file");
                }
                Load(destinationDirectory, Path.GetFileNameWithoutExtension(sourceFile), binaryReader, (uint)new FileInfo(sourceFile).Length); 
            }
        }
    }
}
