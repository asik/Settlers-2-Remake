using System;
using System.IO;
using System.Text;
using System.Linq;


namespace AssetImporter {
    // Extension methods to hack having both Little- and Big-endian support at the same time,
    // and various other utility methods for reading and writing binary data.
    // This class assumes the BinaryReader/BinaryWriter is set to its default, Little_Endian.
    public static class BinaryExtensions {

        // In order to support other types, copy-paste this method and change the types manually.
        // This is BEGGING for a generic implementation, yet
        // there is NO WAY to make this generic in C#, due to the stupid limitations of the type system.
        // (And yeah I've tried unsafe code and every trick in the book)
        // Why does no language ever get generics right?
        public static uint BE_ReadUInt32(this BinaryReader b) {
            uint number = b.ReadUInt32();
            byte[] temp = BitConverter.GetBytes(number);
            Array.Reverse(temp);
            number = BitConverter.ToUInt32(temp, 0);
            return number;
        }

        public static ushort BE_ReadUInt16(this BinaryReader b) {
            ushort number = b.ReadUInt16();
            byte[] temp = BitConverter.GetBytes(number);
            Array.Reverse(temp);
            number = BitConverter.ToUInt16(temp, 0);
            return number;
        }

        public static short BE_ReadInt16(this BinaryReader b) {
            short number = b.ReadInt16();
            byte[] temp = BitConverter.GetBytes(number);
            Array.Reverse(temp);
            number = BitConverter.ToInt16(temp, 0);
            return number;
        }



        public static void BE_Write(this BinaryWriter b, int value) {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            b.Write(bytes);
        }

        public static void BE_Write(this BinaryWriter b, ushort value) {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            b.Write(bytes);
        }

        public static void BE_Write(this BinaryWriter b, uint value) {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            b.Write(bytes);
        }


        public static uint BE_ReadUInt24(this BinaryReader b) {
            byte[] temp = b.ReadBytes(3);
            temp = new byte[4] { temp[2], temp[1], temp[0], 0x00 };
            uint number = BitConverter.ToUInt32(temp, 0);
            return number;
        }

        /// <summary>
        /// Reads a variable-length 32-bit unsigned integer.
        /// </summary>
        /// <param name="b">Explicit "this"</param>
        /// <returns>The value that has been read</returns>
        public static uint ReadUInt32_VariableLength(this BinaryReader b) {
            uint value = 0;
            for (int i = 0; i < 4; ++i) {
                byte nextByte = b.ReadByte();
                value = (value << 7) | (uint)(nextByte & 0x7F);
                if ((nextByte & 0x80) == 0)
                    break;
            }
            return value;
        }

        /// <summary>
        /// Writes a variable-length 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The value to be written</param>
        public static void WriteUInt32_VariableLength(this BinaryWriter b, uint value) {
            int byteCount = 1;
            uint buffer = value & 0x7F;
            for (; (value >>= 7) != 0; ++byteCount) {
                buffer = (buffer << 8) | 0x80 | (value & 0x7F);
            }
            for (int i = 0; i < byteCount; ++i) {
                byte temp = (byte)(buffer & 0xFF);
                b.Write(temp);
                buffer >>= 8;
            }
        }

        /// <summary>
        /// Read a null-terminated ASCII string.
        /// </summary>
        /// <param name="count">Number of bytes to read.</param>
        public static string ReadNullTerminatedASCIIString(this BinaryReader binaryReader, int count) {
            var rawBytes = binaryReader.ReadBytes(count);
            rawBytes = rawBytes.TakeWhile(b => b != 0x00).ToArray();
            var chars = Encoding.ASCII.GetChars(rawBytes);
            return new string(chars);
        }
    }
}
