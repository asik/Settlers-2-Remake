using System;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

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
            float zoom = 1.0f;
            cameraPosition = new Vector3(0, -((cameraHeight * TerrainRenderer.triangleWidth) / TerrainRenderer.triangleHeight), cameraHeight);//new Vector3(0, -cameraHeight * (float)Math.Tan(Math.Asin(TerrainRenderer.triangleHeight / TerrainRenderer.triangleWidth)), cameraHeight);
            cameraTarget = new Vector3(0, 0, 0);
            //projection = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0), (float)ClientSize.Width / (float)ClientSize.Height, 0.001f, 1000.0f);
            projection = Matrix4.CreateOrthographic((float)ClientSize.Width / (50 * zoom), (float)ClientSize.Height / (50 * zoom), 0.001f, 1000.0f);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            terrainRenderer = new TerrainRenderer(currentMap);
        }



        protected override void OnUpdateFrame(FrameEventArgs e) {
            float amount = (float)e.Time * 50;
            if (Keyboard[Key.Up]) {
                MoveCamera(0.0f, amount);
            }
            if (Keyboard[Key.Right]) {
                MoveCamera(amount, 0.0f);
            }
            if (Keyboard[Key.Left]) {
                MoveCamera(-amount, 0.0f);
            }
            if (Keyboard[Key.Down]) {
                MoveCamera(0.0f, -amount);
            }
        }


        void MoveCamera(float x, float y) {
            cameraPosition.X += x;
            cameraPosition.Y += y;
            cameraTarget.X += x;
            cameraTarget.Y += y;
        }


        //float[] vertices = new float[] { 0.0f, 1.0f, -10.0f, -1.0f, -1.0f, -10.0f, 1.0f, -1.0f, -10.0f };
        //float[] colors = new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f };

        protected override void OnRenderFrame(FrameEventArgs e) {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            var modelView = Matrix4.LookAt(cameraPosition, cameraTarget, Vector3.UnitZ);
            GL.LoadMatrix(ref modelView);
            terrainRenderer.Draw();

            SwapBuffers();
        }
    }
}
