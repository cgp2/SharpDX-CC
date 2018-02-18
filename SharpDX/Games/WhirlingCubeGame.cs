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
using System.IO;

namespace SharpDX.Games
{
    class WhirlingCubeGame : Game, IDisposable
    {
        Texture2D backBuffer = null;
     
        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
        };
        protected Direct3D11.Buffer constantBuffer;
        protected Texture2D depthBuffer = null;
        protected DepthStencilView depthView = null;

        CubeComponent cube1;
        CubeComponent cube2;
        CubeComponent cube3;
        CubeComponent cube4;
        GridComponent grid;
        TriangleComponent trg;
        SphereComponent sphere;      

        public WhirlingCubeGame()
        {
            InitializeShaders();

            Camera = new CameraComponent((float)renderForm.Width / renderForm.Height, new Vector3(0, 2, -3), 0, 30);
        }

        new protected void InitializeShaders()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\MiniCube.fx";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);
            

            inputLayoutMain = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayoutMain;


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

            cube1 = new CubeComponent(device);
            cube1.InitialPosition = new Vector3(5f, 0f, 5f);
            cube1.Update();

            cube2 = new CubeComponent(device);
            cube2.InitialPosition = new Vector3(-5f, 0f, -5f);
            cube2.Update();

            cube3 = new CubeComponent(device);
            cube3.InitialPosition = new Vector3(-5f, 5f, 5f);
            cube3.Update();

            cube4 = new CubeComponent(device);
            cube4.InitialPosition = new Vector3(5f, -5f, -5f);
            cube4.Update();

            grid = new GridComponent(device);
            grid.InitialPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            trg = new TriangleComponent(device);
            trg.InitialPosition = new Vector3(0, 0f, 0);
            trg.Update();

            sphere = new SphereComponent(device, 3, 10);
            sphere.InitialPosition = new Vector3(5f, 1f, 0f);
            sphere.Update();
            
            var texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "text.png"));
            ShaderResourceView textureView = new ShaderResourceView(device, texture);

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
            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);

            cube1.Draw(deviceContext, Camera.Proj, Camera.View, true);       

            cube2.Draw(deviceContext, Camera.Proj, Camera.View, true);

            cube3.Draw(deviceContext, Camera.Proj, Camera.View, true);

            cube4.Draw(deviceContext, Camera.Proj, Camera.View, true);
         
            grid.Draw(deviceContext, Camera.Proj, Camera.View, true);
           // trg.Draw(deviceContext, viewProj, constantBuffer);

            sphere.Draw(deviceContext, Camera.Proj, Camera.View, true);

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

