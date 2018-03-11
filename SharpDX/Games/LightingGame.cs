using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SharpDX.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Lighting;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SharpDX.Games
{
    internal class LightingGame : Game, IDisposable
    {
        private const int SMapSize = 2048;

        private readonly InputElement[] inputElements =
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
        };

        private readonly ShadowMap shadowMap;
        private ObjModelComponent cow;
        private CubeComponentTextured cube;

        private Texture2D depthBuffer;
        private DepthStencilView depthView;

        private GridComponentTextured grid;
        private Buffer lightBuf;
        private LightBufferStruct lightBufferStruct;
        private PlaneComponent plane;

        private ShaderResourceView shadowMapResourseView;
        private SoComponent soComp;

        private DirectionalLight directionalLight;

        private Buffer eyeBuf;

        public LightingGame()
        {
            InitializeShaders();
            InitializeBuffers();

            Camera = new CameraComponent((float) RenderForm.Width / RenderForm.Height, new Vector3(0, 7, -12), 180, 0);

            shadowMap = new ShadowMap(GameDevice, SwapChain, SMapSize, SMapSize);
        }

        protected new void InitializeShaders()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location) + "\\Shaders\\ShadowMapLightingShaders.hlsl";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                InputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertexShader = new VertexShader(GameDevice, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                PixelShader = new PixelShader(GameDevice, pixelShaderByteCode);
            }

            InputLayoutMain = new InputLayout(GameDevice, InputSignature, inputElements);

            var rasterizerStateDesc = RasterizerStateDescription.Default();
            rasterizerStateDesc.IsScissorEnabled = false;
            var rasterizerState = new RasterizerState(GameDevice, rasterizerStateDesc);
            DeviceContext.Rasterizer.State = rasterizerState;
            DeviceContext.Rasterizer.SetScissorRectangle(0, 100, Width, Height - 100);
        }

        protected void InitializeBuffers()
        {
            eyeBuf = new Buffer(GameDevice, Utilities.SizeOf<Vector4>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            lightBuf = new Buffer(GameDevice, Utilities.SizeOf<LightBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            depthBuffer = new Texture2D(GameDevice, new Texture2DDescription
            {
                //Format = Format.R32G8X24_Typeless,
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

            depthView = new DepthStencilView(GameDevice, depthBuffer);

            var dssd = DepthStencilStateDescription.Default();
            //dssd.IsDepthEnabled = true;
            //dssd.IsStencilEnabled = true;
            //dssd.DepthWriteMask = DepthWriteMask.All;
            //dssd.DepthComparison = Comparison.Less;
            //dssd.FrontFace = new DepthStencilOperationDescription
            //{
            //    Comparison = Comparison.Equal,
            //    DepthFailOperation = StencilOperation.Keep,
            //    FailOperation = StencilOperation.Keep,
            //    PassOperation = StencilOperation.Keep
            //};
            //dssd.BackFace = new DepthStencilOperationDescription
            //{
            //    Comparison = Comparison.Equal,
            //    DepthFailOperation = StencilOperation.Increment,
            //    FailOperation = StencilOperation.Increment,
            //    PassOperation = StencilOperation.Keep
            //};

            var dss = new DepthStencilState(GameDevice, dssd);
            DeviceContext.OutputMerger.SetDepthStencilState(dss);

            //DSVText = new Texture2D(GameDevice, new Texture2DDescription()
            //{
            //    Format = Format.D32_Float_S8X24_UInt,
            //    ArraySize = 1,
            //    MipLevels = 1,
            //    Width = width,
            //    Height = height,
            //    SampleDescription = new SampleDescription(1, 0),
            //    Usage = ResourceUsage.Default,
            //    BindFlags = BindFlags.DepthStencil,
            //    CpuAccessFlags = CpuAccessFlags.None,
            //    OptionFlags = ResourceOptionFlags.None
            //});

            //// Create the depth buffer view
            //depthMapDSV = new DepthStencilView(GameDevice, DSVText);
        }

        public void Run()
        {
            //grid = new GridComponent(GameDevice);
            grid = new GridComponentTextured(GameDevice, 20, "photo-1491592558635-f4cc68beb2e9.jpg");
            grid.Translation = new Vector3(-grid.Lenght / 4f, 0f, -grid.Lenght / 4f);
            //grid.Translation = new Vector3(0, 0f, -5);
            grid.Update();

            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location) + "\\Cow.obj";
            cow = new ObjModelComponent(GameDevice, path, "bright_silver_foil_cardstock-1_5fd8774c-ce46-414b-b323-2be87c8a37a7_1000x.jpg")
            {
                Translation = new Vector3(0f, 0f, 0f),
                Scaling = new Vector3(0.1f, 0.1f, 0.1f)
            };
            cow.Update();

            //PointLight = new Lighting.PointLight(new Vector4(0f, 5f, 90f, 1f), Color.LightPink, 2f);
            directionalLight = new DirectionalLight(new Vector4(200f, 25f, 0f, 1f), 50, 50, Color.White, 5f);

            lightBufferStruct = new LightBufferStruct
            {
                //Ask Why
                Position = new Vector4(0f, 1f, 0f, 0f),
                Color = directionalLight.Color,
                Intensity = directionalLight.Intensity
            };

            cube = new CubeComponentTextured(GameDevice, "bright_silver_foil_cardstock-1_5fd8774c-ce46-414b-b323-2be87c8a37a7_1000x.jpg")
            {
                InitialPosition = new Vector3(5f, 4f, 2f)
            };
            cube.Update();

            DrawShadowMap();

            plane = new PlaneComponent(GameDevice, shadowMapResourseView)
            {
                Translation = new Vector3(-5f, 5f, 5f),
                Rotation = Matrix.RotationYawPitchRoll(MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(-45),
                    MathUtil.DegreesToRadians(0))
            };
            plane.Update();

            soComp = new SoComponent(GameDevice, cow.Vertices);

            RenderLoop.Run(RenderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        public void Draw()
        {
            soComp.DrawSo(DeviceContext);
            //Draw Shadow Map

            var samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.ComparisonMinMagMipLinear,
                BorderColor = Color.Zero,
                ComparisonFunction = Comparison.Less
            };
            var shadowMapSampler = new SamplerState(GameDevice, samplerStateDescription);

            //Draw main components
            BindComponents();

            DeviceContext.PixelShader.SetSampler(1, shadowMapSampler);

            DeviceContext.VertexShader.Set(VertexShader);
            DeviceContext.PixelShader.Set(PixelShader);
            DeviceContext.GeometryShader.Set(null);

            DeviceContext.PixelShader.SetShaderResource(1, shadowMapResourseView);

            var eyepos = new Vector4(Camera.EyePosition.X, Camera.EyePosition.Y, Camera.EyePosition.Z, 1f);

            DeviceContext.UpdateSubresource(ref eyepos, eyeBuf);
            DeviceContext.PixelShader.SetConstantBuffer(1, eyeBuf);

            DeviceContext.UpdateSubresource(ref lightBufferStruct, lightBuf);
            DeviceContext.PixelShader.SetConstantBuffer(2, lightBuf);

            DeviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            DeviceContext.ClearRenderTargetView(RenderTargetView, Color.Black);

            plane.Draw(DeviceContext, Camera.Proj, Camera.View);
            grid.Draw(DeviceContext, Camera.Proj, Camera.View);

            cow.Draw(DeviceContext, Camera.Proj, Camera.View);

            soComp.DrawPs(DeviceContext, Camera.View, Camera.Proj, Matrix.AffineTransformation(1f, Quaternion.RotationMatrix(cow.Rotation), cow.Translation));

            SwapChain.Present(1, PresentFlags.None);
        }

        public void DrawShadowMap()
        {
            shadowMap.BindComponents(DeviceContext);

            grid.DrawShadow(DeviceContext, directionalLight.ShadowTransform, directionalLight.Projection, directionalLight.View);
            cow.DrawShadow(DeviceContext, directionalLight.ShadowTransform, directionalLight.Projection, directionalLight.View);
            // cobbl.DrawShadow(deviceContext, DirectionalLight.View, DirectionalLight.Projection);

            shadowMapResourseView = new ShaderResourceView(GameDevice, shadowMap.DepthMap);
        }

        public void BindComponents()
        {
            DeviceContext.Rasterizer.SetViewport(Viewport);
            DeviceContext.OutputMerger.SetTargets(depthView, RenderTargetView);

            DeviceContext.InputAssembler.InputLayout = InputLayoutMain;
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

        public void Dispose()
        {
            foreach (var comp in Components)
                comp.Dispose();
            DisposeBase();
            eyeBuf.Dispose();
            lightBuf.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
        }

        public override void MouseMoved(float x, float y)
        {
            Camera.ChangeTargetPosition(x, y);
        }
    }
}