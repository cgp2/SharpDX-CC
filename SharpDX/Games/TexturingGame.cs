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
using SharpDX.Components;

namespace SharpDX.Games
{
    class TexturingGame : Game, IDisposable
    {
        Texture2D backBuffer = null;
     
        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
        };
        protected Direct3D11.Buffer constantBuffer;
        protected Texture2D depthBuffer = null;
        protected DepthStencilView depthView = null;

        TriangleComponent trg;
        CubeComponentTextured cube;
        GridComponentTextured grid;

        public TexturingGame()
        {
            InitializeShaders();
        }

        new protected void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("TextureShaders.hlsl", "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("TextureShaders.hlsl", "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);

            inputLayout = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayout;

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

            renderTargetView = new RenderTargetView(device, backBuffer);
            depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = renderForm.ClientSize.Width,
                Height = renderForm.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            
            depthView = new DepthStencilView(device, depthBuffer);
        }


        public void Run()
        {
            deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
            deviceContext.OutputMerger.SetTargets(depthView, renderTargetView);      

            trg = new TriangleComponent(device);
            trg.WorldPosition = new Vector3(0, 2f, 0);
            trg.Update();
            trg.RotationCenter = trg.WorldPosition;
            //trg.Scaling = Matrix.Scaling(10);

            cube = new CubeComponentTextured(device);
            cube.WorldPosition = new Vector3(5f, 5f, 0);
            cube.RotationCenter = cube.WorldPosition;
            cube.Update();

            grid = new GridComponentTextured(device);
            grid.WorldPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            clock.Start();

            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);       

            Draw();
        }


        public void Draw()
        {
            var time = clock.ElapsedMilliseconds / 1000f;

            //cube.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), (float)Math.Cos(time / 2), (float)Math.Cos(time / 2));

            trg.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), 0, 0);

            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);

            trg.Draw(deviceContext, viewProj, constantBuffer);
            cube.Draw(deviceContext, viewProj, constantBuffer);
            grid.Draw(deviceContext, viewProj, constantBuffer);

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
                case Keys.Escape:
                    Dispose();
                    break;
            }
        }

        public override void MouseMoved(float x, float y)
        {
            Camera.ChangeTargetPosition(x, y);
        }

        public void Dispose()
        {
            DisposeBase();
            constantBuffer.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
        }
    }
}

