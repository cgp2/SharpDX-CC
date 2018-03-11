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

namespace SharpDX.Games
{
    class TriangleGame : Game, IDisposable
    {
        private VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector4(-0.5f, 0.5f, 0.0f, 1f), SharpDX.Color.Red),
                new VertexPositionColor(new Vector4(0.5f, 0.5f, 0.0f, 1f), SharpDX.Color.Blue),
                new VertexPositionColor(new Vector4(0.0f, -0.5f, 0.0f,1f), SharpDX.Color.Green),
            };

        public TriangleGame()
        {
            RenderForm = new RenderForm();
            RenderForm.ClientSize = new Size(Width, Height);
            RenderForm.AllowUserResizing = false;

            InitializeDeviceResources();
            InitializeShaders();
            InitializeTriangle();
        }

        private void InitializeTriangle()
        {
            VertexBuffer = Direct3D11.Buffer.Create(GameDevice, Direct3D11.BindFlags.VertexBuffer, vertices);
        }


        public void Run()
        {
            RenderLoop.Run(RenderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        public void Draw()
        {
            DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            DeviceContext.OutputMerger.SetRenderTargets(RenderTargetView);
            DeviceContext.ClearRenderTargetView(RenderTargetView, Color.Black);

            DeviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
            DeviceContext.Draw(vertices.Count(), 0);

            SwapChain.Present(1, PresentFlags.None);
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
