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
        protected Direct3D11.Buffer ConstantBuffer;
        protected Texture2D DepthBuffer = null;
        protected DepthStencilView DepthView = null;

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

            Camera = new CameraComponent((float)RenderForm.Width / RenderForm.Height, new Vector3(0, 2, -3), 0, 30);
        }

        new protected void InitializeShaders()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\MiniCube.fx";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                InputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertexShader = new Direct3D11.VertexShader(GameDevice, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                PixelShader = new Direct3D11.PixelShader(GameDevice, pixelShaderByteCode);
            }

            DeviceContext.VertexShader.Set(VertexShader);
            DeviceContext.PixelShader.Set(PixelShader);
            

            InputLayoutMain = new InputLayout(GameDevice, InputSignature, inputElements);
            DeviceContext.InputAssembler.InputLayout = InputLayoutMain;


            ConstantBuffer = new Direct3D11.Buffer(GameDevice, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);

            RenderTargetView = new RenderTargetView(GameDevice, backBuffer);
            DepthBuffer = new Texture2D(GameDevice, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = RenderForm.ClientSize.Width,
                Height = RenderForm.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });


            DepthView = new DepthStencilView(GameDevice, DepthBuffer);
        }


        public void Run()
        {
            DeviceContext.Rasterizer.SetViewport(new Viewport(0, 0, RenderForm.ClientSize.Width, RenderForm.ClientSize.Height, 0.0f, 1.0f));
            DeviceContext.OutputMerger.SetTargets(DepthView, RenderTargetView);

            cube1 = new CubeComponent(GameDevice);
            cube1.InitialPosition = new Vector3(5f, 0f, 5f);
            cube1.Update();

            cube2 = new CubeComponent(GameDevice);
            cube2.InitialPosition = new Vector3(-5f, 0f, -5f);
            cube2.Update();

            cube3 = new CubeComponent(GameDevice);
            cube3.InitialPosition = new Vector3(-5f, 5f, 5f);
            cube3.Update();

            cube4 = new CubeComponent(GameDevice);
            cube4.InitialPosition = new Vector3(5f, -5f, -5f);
            cube4.Update();

            grid = new GridComponent(GameDevice);
            grid.InitialPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            trg = new TriangleComponent(GameDevice);
            trg.InitialPosition = new Vector3(0, 0f, 0);
            trg.Update();

            sphere = new SphereComponent(GameDevice, 3, 10);
            sphere.InitialPosition = new Vector3(5f, 1f, 0f);
            sphere.Update();
            
            var texture = TextureLoader.CreateTexture2DFromBitmap(GameDevice, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "text.png"));
            ShaderResourceView textureView = new ShaderResourceView(GameDevice, texture);

            RenderLoop.Run(RenderForm, RenderCallback);
        }

        private void RenderCallback()
        {

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            DeviceContext.UpdateSubresource(ref worldViewProj, ConstantBuffer, 0);
       

            Draw();
        }


        public void Draw()
        {
            DeviceContext.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            DeviceContext.ClearRenderTargetView(RenderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);

            cube1.Draw(DeviceContext, Camera.Proj, Camera.View);       

            cube2.Draw(DeviceContext, Camera.Proj, Camera.View);

            cube3.Draw(DeviceContext, Camera.Proj, Camera.View);

            cube4.Draw(DeviceContext, Camera.Proj, Camera.View);
         
            grid.Draw(DeviceContext, Camera.Proj, Camera.View);
           // trg.Draw(deviceContext, viewProj, ConstantBuffer);

            sphere.Draw(DeviceContext, Camera.Proj, Camera.View);

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
            ConstantBuffer.Dispose();
            DepthBuffer.Dispose();
            DepthView.Dispose();
        }
    }
}

