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
        private const int SMapSize = 2048;
        ShadowMap shadowMap;

        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
        };

        private InputElement[] soInputElement = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
        };

        private Direct3D11.InputElement[] colorInputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
        };

        protected Texture2D depthBuffer;
        protected DepthStencilView depthView;

        Direct3D11.Buffer EyeBuf;
        Direct3D11.Buffer LightBuf;
        Direct3D11.Buffer ShdMapBufView;
        Direct3D11.Buffer ShdMapBufProj;
        Direct3D11.Buffer SOBuffer;

        //GridComponent grid;
        GridComponentTextured grid;
        ObjModelComponent cow;
        PlaneComponent plane;

        Lighting.SpotLight SpotLight;

        ShaderResourceView ShadowMapResourseView;
        LightBufferStruct lightBufferStruct;

        GeometryShader GeometryShader;
        VertexShader vertexShaderSO;
        ShaderSignature soInputSignature;
        InputLayout soInputLayout;

        PixelShader colorPixelShader;
        VertexShader colorVertexShader;
        ShaderSignature colorInputSignature;
        InputLayout colorInputLayout;

        Direct3D11.Buffer constantBufferColor;

        public LightingGame()
        {
            InitializeShaders();
            InitializeBuffers();

            Camera = new CameraComponent((float)renderForm.Width / renderForm.Height, new Vector3(0, 7, -12), 180, 0);

            shadowMap = new ShadowMap(device, swapChain, SMapSize, SMapSize);
        }

        new protected void InitializeShaders()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\ShadowLightingShaders.hlsl";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\MiniCube.fx";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                colorInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                colorVertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                colorPixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\GeometryShader.hlsl";
            using (var geometryShaderByteCode = ShaderBytecode.CompileFromFile(path, "GS", "gs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                StreamOutputElement[] streamOutput = new StreamOutputElement[]
                {
                    new StreamOutputElement
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        StartComponent = 0,
                        ComponentCount = 4,
                        OutputSlot = 0,
                    },

                     new StreamOutputElement
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        StartComponent = 0,
                        ComponentCount = 4,
                        OutputSlot = 0,
                    },
                };

                int[] bufferStride = new int[]
                    {
                        Utilities.SizeOf<Vector4>() + Utilities.SizeOf<Color4>(),
                    };

                GeometryShader = new Direct3D11.GeometryShader(device, geometryShaderByteCode, streamOutput, bufferStride, -1);
            }

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                soInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShaderSO = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            colorInputLayout = new InputLayout(device, colorInputSignature, colorInputElements);
            soInputLayout = new InputLayout(device, soInputSignature, soInputElement);
            inputLayoutMain = new InputLayout(device, inputSignature, inputElements);
        }

        protected void InitializeBuffers()
        {
            EyeBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Vector4>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            LightBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<LightBufferStruct>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShdMapBufView = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShdMapBufProj = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            constantBufferColor = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            SOBuffer = new Direct3D11.Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                StructureByteStride = Utilities.SizeOf<VertexPositionColor>(),
                SizeInBytes = Utilities.SizeOf<VertexPositionColor>() * 100000,
            });

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
            //grid = new GridComponent(device);
            grid = new GridComponentTextured(device, 20, "photo-1491592558635-f4cc68beb2e9.jpg");
            grid.Translation = new Vector3(-grid.Lenght / 4, 0f, -grid.Lenght / 4);
            //grid.Translation = new Vector3(0, 0f, -5);
            grid.Update();

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Cow.obj";
            cow = new ObjModelComponent(device, path, "bright_silver_foil_cardstock-1_5fd8774c-ce46-414b-b323-2be87c8a37a7_1000x.jpg");
            cow.Translation = new Vector3(0f, 0f, 0f);
            cow.Scaling = new Vector3(0.1f, 0.1f, 0.1f);
            cow.Update();

            //location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //path = Path.GetDirectoryName(location).ToString() + "\\Toilet.obj";
            //cobbl = new ObjModelComponent(device, path, "bright_silver_foil_cardstock-1_5fd8774c-ce46-414b-b323-2be87c8a37a7_1000x.jpg");
            //cobbl.Translation = new Vector3(-7f, 7f, -3f);
            //cobbl.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            //cobbl.Update();

            //PointLight = new Lighting.PointLight(new Vector4(0f, 5f, 90f, 1f), Color.LightPink, 2f);
            SpotLight = new Lighting.SpotLight(new Vector4(100f, 10f, 0f, 0f), -90, -30, 5, 90, Color.White, 2f);

            lightBufferStruct = new LightBufferStruct();
            lightBufferStruct.Position = new Vector4(0f, 1f, 0f, 0f);
            lightBufferStruct.Color = SpotLight.Color;
            lightBufferStruct.Intensity = SpotLight.Intensity;

            plane = new PlaneComponent(device, ShadowMapResourseView);
            plane.Translation = new Vector3(-5f, 5f, 5f);
            plane.Rotation = Matrix.RotationYawPitchRoll(MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(-45), MathUtil.DegreesToRadians(0));
            plane.Update();

            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            //deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            Draw();
        }

        public void Draw()
        {
            //Draw to StreamOutput buffer
            deviceContext.StreamOutput.SetTarget(SOBuffer, 0);

            deviceContext.VertexShader.Set(vertexShaderSO);
            deviceContext.PixelShader.Set(null);
            deviceContext.GeometryShader.Set(GeometryShader);
            deviceContext.InputAssembler.InputLayout = soInputLayout;

            cow.Draw(deviceContext, Camera.Proj, Camera.View, true);

            deviceContext.StreamOutput.SetTargets(null);

            //Draw Shadow Map
            DrawShadowMap();
            var samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.ComparisonMinMagMipLinear,
                BorderColor = Color.Zero,
                ComparisonFunction = Comparison.Less

            };
            var shadowMapSampler = new SamplerState(device, samplerStateDescription);          

            //Draw main components
            BindComponents();

            deviceContext.PixelShader.SetSampler(1, shadowMapSampler);

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);
            deviceContext.GeometryShader.Set(null);
            deviceContext.InputAssembler.InputLayout = inputLayoutMain;

            deviceContext.PixelShader.SetShaderResource(1, ShadowMapResourseView);

            var eyepos = new Vector4(Camera.EyePosition.X, Camera.EyePosition.Y, Camera.EyePosition.Z, 1f);

            deviceContext.UpdateSubresource(ref eyepos, EyeBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(1, EyeBuf);

            deviceContext.UpdateSubresource(ref lightBufferStruct, LightBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(2, LightBuf);

            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            plane.Draw(deviceContext, Camera.Proj, Camera.View, false);
            grid.Draw(deviceContext, Camera.Proj, Camera.View, false);

            cow.Draw(deviceContext, Camera.Proj, Camera.View, false);

            //Draw From StreamOutput buffer
            deviceContext.VertexShader.Set(colorVertexShader);
            deviceContext.PixelShader.Set(colorPixelShader);
            deviceContext.InputAssembler.InputLayout = colorInputLayout;

            var viewProj = cow.transform * Camera.View  * Camera.Proj;
            deviceContext.UpdateSubresource(ref viewProj, constantBufferColor, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, constantBufferColor);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleListWithAdjacency;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(SOBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

            deviceContext.DrawAuto();

            swapChain.Present(1, PresentFlags.None);
        }

        public void DrawShadowMap()
        {
            shadowMap.BindComponents(deviceContext);

            deviceContext.UpdateSubresource(ref SpotLight.View, ShdMapBufView, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, ShdMapBufView);

            deviceContext.UpdateSubresource(ref SpotLight.Projection, ShdMapBufProj, 0);
            deviceContext.VertexShader.SetConstantBuffer(1, ShdMapBufProj);

            grid.DrawShadow(deviceContext, SpotLight.ShadowTransform);
            cow.DrawShadow(deviceContext, SpotLight.ShadowTransform);
            // cobbl.DrawShadow(deviceContext, SpotLight.View, SpotLight.Projection);

            ShadowMapResourseView = new ShaderResourceView(device, shadowMap.depthMap);
        }

        public void BindComponents()
        {
            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);

            deviceContext.Rasterizer.SetViewport(viewport);
            deviceContext.OutputMerger.SetTargets(depthView, renderTargetView);

            deviceContext.InputAssembler.InputLayout = inputLayoutMain;
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
            foreach (AbstractComponent comp in components)
                comp.Dispose();
            DisposeBase();
            EyeBuf.Dispose();
            LightBuf.Dispose();
            ShdMapBufView.Dispose();
            ShdMapBufProj.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
        }

        struct LightBufferStruct
        {
            public Vector4 Position;
            public Vector4 Color;
            public float Intensity;
            public float dum1, dum2, dum3;
        }
    }
}

