using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;

namespace Game {
    class GameManager : GameWindow {
        const float cameraHeight = 32;
        public static readonly string BaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
        public static readonly string AssetDirectory = BaseDirectory + "/assets";

        GameKeyboard keyboard;
        Map currentMap;
        Vector3 cameraPosition;
        Vector3 cameraTarget;
        Matrix4 projection;
        TerrainRenderer terrainRenderer;

        public GameManager(int width, int height)
            : base(width, height) {
        }

        protected override void OnLoad(EventArgs e) {
            keyboard = new GameKeyboard(Keyboard);
            currentMap = new Map(AssetDirectory + "/DATA/MAPS4/MAP01.SWD");
            GL.ClearColor(Color.Black);
            cameraPosition = new Vector3(0, -cameraHeight * (float)Math.Tan(Math.Asin(TerrainRenderer.triangleHeight / TerrainRenderer.triangleWidth)), cameraHeight);
            cameraTarget = new Vector3(0, 0, 0);
            //projection = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0), (float)ClientSize.Width / (float)ClientSize.Height, 0.001f, 1000.0f);
            projection = Matrix4.CreateOrthographic((float)ClientSize.Width / TerrainRenderer.triangleWidth, (float)ClientSize.Height / TerrainRenderer.triangleHeight, 0.001f, 1000.0f);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref projection);
            terrainRenderer = new TerrainRenderer(currentMap);
        }



        protected override void OnUpdateFrame(FrameEventArgs e) {
            float amount = (float)e.Time * 100;
            if (keyboard.IsKeyDown(Key.Up)) {
                MoveCamera(0.0f, amount);
            }
            if (keyboard.IsKeyDown(Key.Right)) {
                MoveCamera(amount, 0.0f);
            }
            if (keyboard.IsKeyDown(Key.Left)) {
                MoveCamera(-amount, 0.0f);
            }
            if (keyboard.IsKeyDown(Key.Down)) {
                MoveCamera(0.0f, -amount);
            }
        }


        void MoveCamera(float x, float y) {
            cameraPosition.X += x;
            cameraPosition.Y += y;
            cameraTarget.X += x;
            cameraTarget.Y += y;
        }


        float[] vertices = new float[] { 0.0f, 1.0f, -10.0f, -1.0f, -1.0f, -10.0f, 1.0f, -1.0f, -10.0f };
        float[] colors = new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f };

        protected override void OnRenderFrame(FrameEventArgs e) {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            var modelView = Matrix4.LookAt(cameraPosition, cameraTarget, Vector3.UnitZ);
            GL.LoadIdentity();
            GL.LoadMatrix(ref modelView);
            terrainRenderer.Draw();

            SwapBuffers();

        }

    }
}
