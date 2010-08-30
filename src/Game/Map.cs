using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AssetImporter;
using OpenTK;

namespace Game {

    public enum MapType {
        Greenland,
        Wasteland,
        Snowland
    }

    public enum MapLayerType {
        Altitude,
        Terrain1,
        Terrain2,
        Roads,
        Landscape,
        Type,
        Animals,
        Unknown1,
        BQ,
        Unknown2,
        Unknown3,
        Resources,
        Shadows,
        Lakes
    }

    public enum TerrainType {
        Snow,
        Desert,
        Swamp,
        Meadow_flowers,
        Mountain1,
        Mountain2,
        Mountain3,
        Mountain4,
        Savannah,
        Meadow1,
        Meadow2,
        Meadow3,
        Steppe,
        MountainMeadow,
        Water,
        Lava
    };

    public class MapLayer {
        public int Width;
        public int Height;
        public byte[] Data;
    }

    public class Map {

        public string FilePath { get; private set; }
        public string Name { get; private set; }
        public string Author { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public MapType Type { get; private set; }
        public int NumPlayers { get; private set; }

        Dictionary<MapLayerType, MapLayer> layers;
        int NUM_LAYERS = Enum.GetValues(typeof(MapLayerType)).Length;

        public Map(string filePath) {
            FilePath = filePath;
            layers = new Dictionary<MapLayerType, MapLayer>(NUM_LAYERS);
            using (var binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open))) {
                Load(binaryReader);
            }
        }


        public byte GetData(MapLayerType layerType, int x, int y) {
            return layers[layerType].Data[y * Width + x];
        }

        void Load(BinaryReader binaryReader) {
            ReadHeader(binaryReader);
            // Skip unknown data
            binaryReader.BaseStream.Seek(2296, SeekOrigin.Current);
            for (int i = 0; i < NUM_LAYERS; ++i) {
                ReadLayer(binaryReader, (MapLayerType)i);
            }
        }

        void ReadLayer(BinaryReader binaryReader, MapLayerType type) {
            binaryReader.BaseStream.Seek(6, SeekOrigin.Current);
            var layer = new MapLayer();
            int seek = -4;
            while (layer.Width == 0) {
                layer.Width = binaryReader.ReadUInt16();
                seek += 2;
            }
            while (layer.Height == 0) {
                layer.Height = binaryReader.ReadUInt16();
                seek += 2;
            }
            binaryReader.BaseStream.Seek(6 - seek, SeekOrigin.Current);

            layer.Data = binaryReader.ReadBytes(layer.Width * layer.Height);
            layers[type] = layer;
        }


        void ReadHeader(BinaryReader binaryReader) {
            var id = binaryReader.ReadNullTerminatedASCIIString(10);
            if (id != "WORLD_V1.0") {
                throw new Exception("Invalid header on map file " + FilePath);
            }
            Name = binaryReader.ReadNullTerminatedASCIIString(20);
            Width = binaryReader.ReadUInt16();
            Height = binaryReader.ReadUInt16();
            Type = (MapType)binaryReader.ReadByte();
            NumPlayers = binaryReader.ReadByte();
            Author = binaryReader.ReadNullTerminatedASCIIString(20);
        }


    }
}
