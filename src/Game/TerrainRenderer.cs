using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using TexLib;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Game {

    public struct Vector2i {
        public int X;
        public int Y;
        public Vector2i(int x, int y) { X = x; Y = y; }
        public Vector2 ToVector2() {
            return new Vector2((float)X, (float)Y);
        }
    }

    public struct Triangle2i {
        public Vector2i P0;
        public Vector2i P1;
        public Vector2i P2;
        public Triangle2i(Vector2i p0, Vector2i p1, Vector2i p2) {
            P0 = p0; P1 = p1; P2 = p2;
        }
    }

    public struct Triangle {
        public Vector3 P0;
        public Vector3 P1;
        public Vector3 P2;
        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2) {
            P0 = p0;
            P1 = p1;
            P2 = p2;
        }
    }


    class TerrainRenderer {

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        struct T2fN3fV3f {
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Position;
        }

        public static readonly float triangleWidth = 53;
        public static readonly float triangleHeight = 29;
        static readonly Dictionary<MapType, string> mapTypeAssets = new Dictionary<MapType, string>() {
            { MapType.Greenland, GameManager.AssetDirectory + "/GFX/TEXTURES/TEX5.png" },
            { MapType.Wasteland, GameManager.AssetDirectory + "/GFX/TEXTURES/TEX6.png" },
            { MapType.Snowland, GameManager.AssetDirectory + "/GFX/TEXTURES/TEX7.png" }
        };
        static readonly int TerrainTextureSize = 48; // for most types of terrain
        static readonly Point SpecialTextureSize = new Point(55, 56); // for water and lava

        /// <summary>
        /// Where the textures are located within the texture files. 
        /// For the normal terrain types, multiply offsets by TerrainTextureSize.
        /// For water and lava, the offsets are pre-calculated.
        /// </summary>
        static readonly Dictionary<TerrainType, Point> textureOffsets = new Dictionary<TerrainType, Point>() {
            { TerrainType.Snow,             new Point(0, 0) },
            { TerrainType.Desert,           new Point(1, 0) },
            { TerrainType.Swamp,            new Point(2, 0) },
            { TerrainType.Meadow_flowers,   new Point(3, 0) },
            { TerrainType.Mountain1,        new Point(0, 1) },
            { TerrainType.Mountain2,        new Point(1, 1) },
            { TerrainType.Mountain3,        new Point(2, 1) },
            { TerrainType.Mountain4,        new Point(3, 1) },
            { TerrainType.Savannah,         new Point(0, 2) },
            { TerrainType.Meadow1,          new Point(1, 2) },
            { TerrainType.Meadow2,          new Point(2, 2) },
            { TerrainType.Meadow3,          new Point(3, 2) },
            { TerrainType.Steppe,           new Point(0, 3) },
            { TerrainType.MountainMeadow,   new Point(1, 3) },
            { TerrainType.Water,            new Point(4 * TerrainTextureSize, TerrainTextureSize) },
            { TerrainType.Lava,             new Point(4 * TerrainTextureSize, TerrainTextureSize + SpecialTextureSize.Y) },
        };


        Map map;
        Vector3[,] normals;
        Vector3[] glNormals;
        Vector2[] glTextureCoordinates;
        Triangle[] glTriangles;
        Dictionary<TerrainType, int> glTextures;
        TerrainType[] texturesPerTriangle;
        uint[] vbos;
        Dictionary<TerrainType, T2fN3fV3f[]> sortedVertices;

        public TerrainRenderer(Map map) {
            this.map = map;
            generateNormals();
            generateTriangles();
            generateTextures();
            //generateTextureCoordinates();
            generateSortedVertices();
            generateVBOs();
        }

        void generateTextureCoordinates() {
            glTextureCoordinates = new Vector2[glTriangles.Length * 3];
            //for(int i = 0; i < glTriangles.Length; ++i) {
            //    glTextureCoordinates[3 * i] = new Vector2(0, 0);
            //    glTextureCoordinates[3 * i + 1] = new Vector2(0.5f, 0.5f);
            //    glTextureCoordinates[3 * i + 2] = new Vector2(1, 0.5f);
            //}
            for (int y = 0; y < map.Height; ++y) {
                for (int x = 0; x < map.Width; ++x) {

                }
            }
        }


        void generateSortedVertices() {
            sortedVertices = new Dictionary<TerrainType, T2fN3fV3f[]>();
            var tempLists = new Dictionary<TerrainType, List<T2fN3fV3f>>();

            for (int i = 0; i < glTriangles.Length; ++i) {
                var currentTexture = texturesPerTriangle[i];
                if (!tempLists.Keys.Contains(currentTexture)) {
                    tempLists[currentTexture] = new List<T2fN3fV3f>();
                }
                tempLists[currentTexture].AddRange ( new[] { 
                    new T2fN3fV3f {
                        Normal = glNormals[3 * i],
                        TexCoord = glTextureCoordinates[3 * i],
                        Position = glTriangles[i].P0
                    },
                    new T2fN3fV3f {
                        Normal = glNormals[3 * i + 1],
                        TexCoord = glTextureCoordinates[3 * i + 1],
                        Position = glTriangles[i].P1
                    },
                    new T2fN3fV3f {
                        Normal = glNormals[3 * i + 2],
                        TexCoord = glTextureCoordinates[3 * i + 2],
                        Position = glTriangles[i].P2
                    },
                });
            }
            foreach (var terrainType in tempLists.Keys) {
                sortedVertices[terrainType] = tempLists[terrainType].ToArray();
            }
        }


        void generateVBOs() {
            vbos = new uint[2 * sortedVertices.Keys.Count];
            int i = 0;
            GL.GenBuffers(vbos.Length, vbos);

            foreach (var terrain in sortedVertices.Keys) {
                int actualSize = sortedVertices[terrain].Length * BlittableValueType.StrideOf(sortedVertices[terrain]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbos[i++]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)actualSize, sortedVertices[terrain], BufferUsageHint.StaticDraw);
                int size;
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                if (actualSize != size) {
                    throw new Exception("Vertex data not uploaded correctly!");
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbos[i++]);
                var indices = Enumerable.Range(0, sortedVertices[terrain].Length).ToArray();
                actualSize = indices.Length * sizeof(int);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)actualSize, indices, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
                if (actualSize != size) {
                    throw new Exception("Vertex data not uploaded correctly!");
                }
            }
        }

        public void Draw() {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);
            GL.Enable(EnableCap.Texture2D);

            GL.ShadeModel(ShadingModel.Flat);
            GL.Light(LightName.Light0, LightParameter.Ambient, new Color4(127, 127, 127, (sbyte)0));
            //GL.Light(LightName.Light0, LightParameter.Diffuse, new Color4(200, 200, 200, sbyte.MaxValue));
            //GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1, 1, -1, 0));
            //GL.Light(LightName.Light1, LightParameter.Ambient, new Color4(0, 0, 0, byte.MaxValue));
            //GL.Light(LightName.Light1, LightParameter.Diffuse, new Color4(135, 128, 128, byte.MaxValue));
            //GL.Light(LightName.Light1, LightParameter.Position, new Vector4(-1, -1, -1, 0));

            //renderImmediate();
            renderWithVBOs();
        }


        void renderImmediate() {
            foreach (var texture in sortedVertices.Keys) {
                GL.BindTexture(TextureTarget.Texture2D, (int)texture);
                var element = sortedVertices[texture];
                var numElements = element.Length;
                GL.Begin(BeginMode.Triangles);
                for (int i = 0; i < numElements; ++i) {
                    GL.Normal3(element[i].Normal);
                    GL.TexCoord2(element[i].TexCoord);
                    GL.Vertex3(element[i].Position);
                }
                GL.End();
                Debug.WriteLine(texture);
            }
        }

        void renderWithVBOs() {
            int i = 0;
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Decal);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.RgbScale, 1.0f);
            GL.Disable(EnableCap.Blend);
            foreach (var terrain in sortedVertices.Keys) {
                GL.BindTexture(TextureTarget.Texture2D, glTextures[terrain]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbos[i++]);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbos[i++]);
                GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, 0, (IntPtr)null);
                GL.DrawElements(BeginMode.Triangles, sortedVertices[terrain].Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        void generateTextures() {
            var assetName = mapTypeAssets[map.Type];
            var bitmap = new Bitmap(assetName);
            glTextures = new Dictionary<TerrainType, int>();
            TexUtil.InitTexturing();
            foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType))) {
                var offsets = textureOffsets[terrainType];
                var size = new Size(SpecialTextureSize.X, SpecialTextureSize.Y);
                if (terrainType != TerrainType.Water && terrainType != TerrainType.Lava) {
                    offsets.X *= TerrainTextureSize;
                    offsets.Y *= TerrainTextureSize;
                    size.Width = size.Height = TerrainTextureSize;
                }
                var subBitmap = bitmap.Clone(new Rectangle(offsets, size), bitmap.PixelFormat);
                glTextures[terrainType] = TexUtil.CreateTextureFromBitmap(subBitmap);                
            }
            GL.Disable(EnableCap.Texture2D);
        }

        void generateNormals() {
            normals = new Vector3[map.Height, map.Width];
            Vector3 v1, v2, v3, v4, v5, v6;
            Vector3 n, n1, n2, n3, n4, n5, n6;
            for (int y = 0; y < map.Height; ++y) {
                for (int x = 0; x < map.Width; ++x) {
                    if (y == 0 && x == 0) {
                        // back left corner - 1 tri 2 vertices
                        v1 = GetMapVertex(x, y + 1) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        n = Vector3.Cross(v1, v2);
                    }
                    else if ((y > 0 && y < (map.Height - 1)) && x == 0) {
                        // left edge - 3 tri 4 vertices
                        v1 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x + 1, y - 1) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3); n3 = Vector3.Cross(v3, v4);
                        n = (n1 + n2 + n3) / 3.0f;
                    }
                    else if (y == (map.Height - 1) && x == 0) {
                        // front left corner - 2 tri 3 vertices
                        v1 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x + 1, y - 1) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3);
                        n = (n1 + n2) / 2.0f;
                    }
                    else if (y == (map.Height - 1) && (x > 0 && x < (map.Width - 1))) {
                        // front edge - 3 tri 4 vertices
                        v1 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x - 1, y - 1) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3); n3 = Vector3.Cross(v3, v4);
                        n = (n1 + n2 + n3) / 3.0f;
                    }
                    else if (y == (map.Height - 1) && x == (map.Width - 1)) {
                        // front right corner - 1 tri 2 vertices
                        v1 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2);
                        n = n1;
                    }
                    else if ((y > 0 && y < (map.Height - 1)) && x == (map.Width - 1)) {
                        // right edge - 3 tri 4 vertices
                        v1 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x - 1, y + 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x, y + 1) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3); n3 = Vector3.Cross(v3, v4);
                        n = (n1 + n2 + n3) / 3.0f;
                    }
                    else if (y == 0 && x == (map.Width - 1)) {
                        // back right corner - 2 tri 3 vertices
                        v1 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x - 1, y + 1) - GetMapVertex(x - 1, y);
                        v3 = GetMapVertex(x - 1, y + 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x, y + 1) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v3, v4);
                        n = (n1 + n2) / 2.0f;
                    }
                    else if (y == 0 && (x > 0 && x < (map.Width - 1))) {
                        // back edge - 3 tri 4 vertices
                        v1 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x - 1, y + 1) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x, y + 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3); n3 = Vector3.Cross(v3, v4);
                        n = (n1 + n2 + n3) / 3.0f;
                    }
                    else {
                        // internal - 6 tri 6 vertices
                        v1 = GetMapVertex(x + 1, y) - GetMapVertex(x, y);
                        v2 = GetMapVertex(x, y - 1 + 1) - GetMapVertex(x, y);
                        v3 = GetMapVertex(x, y - 1) - GetMapVertex(x, y);
                        v4 = GetMapVertex(x - 1, y) - GetMapVertex(x, y);
                        v5 = GetMapVertex(x - 1, y + 1) - GetMapVertex(x, y);
                        v6 = GetMapVertex(x, y + 1) - GetMapVertex(x, y);
                        n1 = Vector3.Cross(v1, v2); n2 = Vector3.Cross(v2, v3); n3 = Vector3.Cross(v3, v4);
                        n4 = Vector3.Cross(v4, v5); n5 = Vector3.Cross(v5, v6); n6 = Vector3.Cross(v6, v1);
                        n = (n1 + n2 + n3 + n4 + n5 + n6) / 6.0f;
                    }
                    n.Normalize();
                    normals[y, x] = n;
                }
            }
        }

        void generateTriangles() {
            var triangleList = new List<Triangle>();
            var normalList = new List<Vector3>();
            var textureList = new List<TerrainType>();
            var texCoordList = new List<Vector2>();
            // Given a square A, B, C, D, the four summits are
            var texCoordA = new Vector2(0, 0);
            var texCoordB = new Vector2(0, 1);
            var texCoordC = new Vector2(1, 1);
            var texCoordD = new Vector2(1, 0);
            // Texture coordinates for a triangle pointing down
            var tdLeft = new Vector2(0, 0.5f);
            var tdRight = new Vector2(0.5f, 0.5f);//0.47f, 0f);
            var tdDown = new Vector2(0.25f, 0f);//0.225f, 0.45f);
            // Texture coordinates for a triangle pointing up
            var tuUp = new Vector2(0.25f, 0.5f);//0.225f, 0);
            var tuLeft = new Vector2(0f, 0f);//0, 0.45f);
            var tuRight = new Vector2(0.5f, 0f);//0.45f, 0.45f);

            // Textures for lava and water are actually rotated
            var texCoordWA = new Vector2(0.5f, 0);
            var texCoordWB = new Vector2(0, 0.5f);
            var texCoordWC = new Vector2(0.5f, 1);
            var texCoordWD = new Vector2(1, 0.5f);

            for (int y = 0; y < map.Height - 1; ++y) {
                for (int x = 0; x < map.Width; ++x) {
                    if ((y & 1) == 0) {
                        // Even rows
                            if (x > 0) {
                                // vertices
                                //var v0 = new Vector2i(x, y);
                                //var v1 = new Vector2i(x - 1, y + 1);
                                //var v2 = new Vector2i(x, y + 1);
                                addTriangle(triangleList, normalList, textureList, MapLayerType.Terrain1, x, y, x - 1, y + 1, x, y + 1);
                                if (isLavaOrWater(x, y, MapLayerType.Terrain1)) {
                                    texCoordList.AddRange(new[] { texCoordWD, texCoordWB, texCoordWC });
                                }
                                else {
                                    texCoordList.AddRange(new[] { tuUp, tuLeft, tuRight });//new[] { texCoordD, texCoordB, texCoordC });
                                }
                            }
                            if (x < map.Width - 1) {
                                // vertices
                                //var v0 = new Vector2i(x, y);
                                //var v1 = new Vector2i(x, y + 1);
                                //var v2 = new Vector2i(x + 1, y);
                                addTriangle(triangleList, normalList, textureList, MapLayerType.Terrain2, x, y, x, y + 1, x + 1, y);
                                if (isLavaOrWater(x, y, MapLayerType.Terrain2)) {
                                    texCoordList.AddRange(new[] { texCoordWA, texCoordWB, texCoordWD });
                                }
                                else {
                                    texCoordList.AddRange(new[] { tdLeft, tdDown, tdRight });//texCoordA, texCoordB, texCoordD });
                                }
                            }
                    }
                    else {
                        // Odd rows
                        if (x < map.Width - 1) {
                            addTriangle(triangleList, normalList, textureList, MapLayerType.Terrain1, x, y, x, y + 1, x + 1, y + 1);
                            addTriangle(triangleList, normalList, textureList, MapLayerType.Terrain2, x, y, x + 1, y + 1, x + 1, y);
                            if (isLavaOrWater(x, y, MapLayerType.Terrain1)) {
                                texCoordList.AddRange(new[] { texCoordWD, texCoordWB, texCoordWC });
                            }
                            else {
                                texCoordList.AddRange(new[] { tuUp, tuLeft, tuRight });
                            }
                            if (isLavaOrWater(x, y, MapLayerType.Terrain2)) {
                                texCoordList.AddRange(new[] { texCoordWA, texCoordWB, texCoordWD });
                            }
                            else {
                                texCoordList.AddRange(new[] { tdLeft, tdDown, tdRight });
                            }
                            //texCoordList.AddRange(new[] { texCoordD, texCoordB, texCoordC, texCoordA, texCoordB, texCoordD });
                        }
                    }
                }
            }
            glTriangles = triangleList.ToArray();
            glNormals = normalList.ToArray();
            texturesPerTriangle = textureList.ToArray();
            glTextureCoordinates = texCoordList.ToArray();
        }

        bool isLavaOrWater(int x, int y, MapLayerType layerType) {
            var terrainType = (TerrainType)map.GetData(layerType, x, y);
            return terrainType == TerrainType.Lava || terrainType == TerrainType.Water;
        }

        //void addTriangle(List<Triangle> triangleList, List<Vector3> normalList, List<TerrainType> textureList, MapLayerType layer,
        //    Triangle2i triangle) {
        //        addTriangle(triangleList, normalList, textureList, layer, 
        //            triangle.P0.X, triangle.P0.Y, 
        //            triangle.P1.X, triangle.P1.Y, 
        //            triangle.P2.X, triangle.P2.Y);
        //}

        void addTriangle(List<Triangle> triangleList, List<Vector3> normalList, List<TerrainType> textureList, MapLayerType layer,
            int p0x, int p0y, int p1x, int p1y, int p2x, int p2y) {
            triangleList.Add(new Triangle {
                P0 = GetMapVertex(p0x, p0y),
                P1 = GetMapVertex(p1x, p1y),
                P2 = GetMapVertex(p2x, p2y)
            });
            normalList.Add(normals[p0y, p0x]);
            normalList.Add(normals[p1y, p1x]);
            normalList.Add(normals[p2y, p2x]);
            //textureList.Add(TerrainType.Swamp);
            textureList.Add((TerrainType)map.GetData(layer, p0x, p0y));
        }
        
        /// <summary>
        /// Generates the position of the vertex for map coords x, y.
        /// Shifts x for uneven rows; 
        /// Inverts y;
        /// Gets z from map data.
        /// </summary>
        Vector3 GetMapVertex(int x, int y) {
            return new Vector3(
                x + ((y & 1) == 0 ? 0.0f : 0.5f),
                map.Height - y - 1,
                adjustedHeight(map.GetData(MapLayerType.Altitude, x, y))
            );
        }
        
        float adjustedHeight(byte rawAltitude) {
            float divisor = triangleHeight * triangleHeight / triangleWidth;
            return (-50 + (2 * rawAltitude)) / divisor;
        }
    }
}
