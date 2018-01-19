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
    class LightingGame : Game, IDisposable
    {
        Texture2D backBuffer = null;

        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
        };

        protected Direct3D11.Buffer constantBuffer;
        protected Texture2D depthBuffer = null;
        protected DepthStencilView depthView = null;

        Direct3D11.Buffer EyeBuf;
        Direct3D11.Buffer LightBuf;

        //GridComponent grid;
        GridComponentTextured grid;
        ObjModelComponent cow;

        Lighting.PointLight PointLight;
    
        List<AbstractComponent> collectedComponents = new List<AbstractComponent>();

        public LightingGame()
        {
            InitializeShaders();

            Camera = new CameraComponent((float)renderForm.Width / renderForm.Height, new Vector3(3, 2, -1), 90, 0);
        }

        new protected void InitializeShaders()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\LightingShaders.hlsl";
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

            inputLayout = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayout;

            EyeBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            LightBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

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

            //grid = new GridComponent(device);
            grid = new GridComponentTextured(device, "hKekH.png");
            grid.InitialPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Cow.obj";
            cow = new ObjModelComponent(device, path, "bright_silver_foil_cardstock-1_5fd8774c-ce46-414b-b323-2be87c8a37a7_1000x.jpg");
            cow.Translation = new Vector3(0f, 0f, 0f);
            cow.Scaling = new Vector3(0.1f, 0.1f, 0.1f);
            cow.Update();

            PointLight = new Lighting.PointLight(new Vector4(0f, 10f, 0f, 0f), Color.White, 1f);

            clock.Start();
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            //deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            Draw();
        }


     
        public void Draw()
        {
            var eyepos = new Vector4(Camera.EyePosition.X, Camera.EyePosition.Y, Camera.EyePosition.Z, 0f);

            deviceContext.UpdateSubresource(ref eyepos, EyeBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(1, EyeBuf);
          
            LightBufferStruct lightBufferStruct = new LightBufferStruct();
            lightBufferStruct.Position = PointLight.WorldPosition;
            lightBufferStruct.Color = PointLight.Color;
            lightBufferStruct.Intensity = PointLight.Intensity;

            deviceContext.UpdateSubresource(ref lightBufferStruct, LightBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(2, LightBuf);

            var time = clock.ElapsedMilliseconds / 1000f;

            //cube.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), (float)Math.Cos(time / 2), (float)Math.Cos(time / 2));

            //trg.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), 0, 0);

            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
        
            grid.Draw(deviceContext, Camera.Proj, Camera.View, constantBuffer);

            cow.Draw(deviceContext, Camera.Proj, Camera.View, constantBuffer);


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
                    Camera.MoveRightAxis();
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
            foreach (AbstractComponent comp in components)
            comp.Dispose();    
            DisposeBase();
            EyeBuf.Dispose();
            LightBuf.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();

        }

        struct LightBufferStruct
        {
            public Vector4 Position;
            public Vector4 Color;
            public float Intensity;
        }
    }
}

