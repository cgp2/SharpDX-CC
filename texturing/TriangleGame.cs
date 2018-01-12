using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;
using System.Windows.Forms;
using Vector2 = SharpDX.Vector2;
using Direct3D11 = SharpDX.Direct3D11;
using Direct3D = SharpDX.Direct3D;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Vector4 = SharpDX.Vector4;
using Vector3 = SharpDX.Vector3;
using Color = SharpDX.Color;
using Matrix = SharpDX.Matrix;
using Quaternion = SharpDX.Quaternion;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace texturing
{
    class TriangleGame : Game, IDisposable
    {
        private VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), SharpDX.Color.Red),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), SharpDX.Color.Blue),
                new VertexPositionColor(new Vector3(0.0f, -0.5f, 0.0f), SharpDX.Color.Green),
            };

        public TriangleGame()
        {
            renderForm = new RenderForm();
            renderForm.ClientSize = new Size(width, height);
            renderForm.AllowUserResizing = false;

            InitializeDeviceResources();
            InitializeShaders();
            InitializeTriangle();
        }

        private void InitializeTriangle()
        {
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
        }


        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        public void Draw()
        {
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
            deviceContext.Draw(vertices.Count(), 0);

            swapChain.Present(1, PresentFlags.None);
        }

        public override void KeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    Camera.MoveForward();
                    break;
                case Keys.S:
                    Camera.MoveBackward();
                    break;
                case Keys.A:
                    Camera.MoveLeft();
                    break;
                case Keys.D:
                    Camera.MoveRight();
                    break;
            }
        }

        public void Dispose()
        {
            DisposeBase();
        }

        public override void MouseMoved(float x, float y)
        {
            throw new NotImplementedException();
        }
    }
}