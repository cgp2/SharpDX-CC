using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SharpDX.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Lighting;
using SharpDX.Materials;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using Filter = SharpDX.Direct3D11.Filter;
using FbxNative;

namespace SharpDX.Games
{
    internal class ShadowMapLightingGame : Game, IDisposable
    {
        private const int SMapSize = 2048;

        private readonly ShadowMap shadowMap;
        private ObjModelComponent cow;
        private CubeComponentTextured cube;
        private FBXComponent skySphere;

        private GridComponentTextured grid;
        private Buffer lightBuf;
        private LightBufferStruct lightBufferStruct;
        private PlaneComponent plane;

        private ShaderResourceView shadowMapResourseView;
        private SoComponent soComp;

        private DirectionalLight directionalLight;

        private Buffer eyeBuf;

        private readonly Texture2D worldPositionMap;
        private readonly Texture2D DiffuseMap;
        private readonly Texture2D DepthMap;
        private readonly Texture2D SpecularMap;
        private readonly Texture2D NormalMap;
        public RenderTargetView[] DefferedRenderTargets;
        public Texture2D[] DefferedTargetTextures;
        private readonly RenderTargetView[] forwardRenderTargets;   

        private Viewport vpD;
        private DepthStencilView dpv;
        private Texture2D PlaneTexture;
        public ConsoleComponent consoleComponent;


        public LuaScriptManager LuaManager;



        public ShadowMapLightingGame()
        {
            vpD = new Viewport(0, 0, 2048, 2048);
           // InitializeShaders();
            Camera = new CameraComponent((float) RenderForm.Width / RenderForm.Height, new Vector3(0, 7, -12), 180, 0);
            shadowMap = new ShadowMap(GameDevice, SwapChain, SMapSize, SMapSize);

            var defferedMapDescr = new Texture2DDescription
            {
                Width = 514,
                Height = 514,
                MipLevels = 1,
                ArraySize = 1,
                //Format = Format.R24G8_Typeless,
                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                //BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            DiffuseMap = new Texture2D(GameDevice, defferedMapDescr);
            DepthMap = new Texture2D(GameDevice, defferedMapDescr);
            SpecularMap = new Texture2D(GameDevice, defferedMapDescr);
            NormalMap = new Texture2D(GameDevice, defferedMapDescr);
            worldPositionMap = new Texture2D(GameDevice, defferedMapDescr); 

           var dd = new Texture2D(GameDevice, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = 600,
                Height = 600,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
             
            // Create the depth buffer view
            dpv = new DepthStencilView(GameDevice, dd);

            DefferedRenderTargets = new[]
            {
                new RenderTargetView(GameDevice, worldPositionMap),
                new RenderTargetView(GameDevice, DiffuseMap),
                new RenderTargetView(GameDevice, DepthMap),
                new RenderTargetView(GameDevice, SpecularMap),
                new RenderTargetView(GameDevice, NormalMap),
            };

            DefferedTargetTextures = new[]
            {
                worldPositionMap,
                DiffuseMap,
                DepthMap,
                SpecularMap,
                NormalMap,
            };

            forwardRenderTargets = new[]
            {
                RenderTargetView
            };

            PlaneTexture = worldPositionMap;
        }

     

        public override void InitializeLight()
        {
            directionalLight = new DirectionalLight(new Vector4(80f, 10f, 0f, 1f), 50, 50, Color.White, 10f);

            lightBufferStruct = new LightBufferStruct
            {
                Position = directionalLight.WorldPosition,
                Color = directionalLight.Color,
                Intensity = directionalLight.Intensity
            };
        }

        public override void InitializeComponents()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var shadersPath = Path.GetDirectoryName(location) + "\\Shaders\\ShadowMapLightingShaders.hlsl";

            grid = new GridComponentTextured(GameDevice);
            var texturePath = Path.GetDirectoryName(location) + "\\Textures\\marble_texture2.jpg";
            grid.InitializeResources(texturePath, new BlackPlastic(), shadersPath, shadersPath, InputElements.VertexPosNormTex);
            grid.Translation = new Vector3(-grid.Lenght / 2f, 0f, -grid.Lenght / 2f);

            cow = new ObjModelComponent(GameDevice);
            texturePath = Path.GetDirectoryName(location) + "\\Textures\\cow_texture.jpg";
            var filePath = Path.GetDirectoryName(location) + "\\Meshes\\cow.obj";
            cow.InitializeResources(texturePath, new Silver(), shadersPath, shadersPath, InputElements.VertexPosNormTex, filePath);
            cow.Scaling = new Vector3(0.01f, 0.01f, 0.01f);





            string[] cubetexturesPath = new string[]{
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_bk.bmp",
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_dn.bmp",
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_ft.bmp",
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_lf.bmp",
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_rt.bmp",
                Path.GetDirectoryName(location) + "\\Textures\\SkyText\\miramar_up.bmp",
            };
            skySphere = new FBXComponent(GameDevice);
            filePath = Path.GetDirectoryName(location) + "\\Meshes\\skySphere2.fbx";
            skySphere.InitializeResources(cubetexturesPath, shadersPath, shadersPath, InputElements.VertexPosNormTex, filePath);
            skySphere.Scaling = new Vector3(0.1f, 0.1f, 0.1f);

            plane = new PlaneComponent(GameDevice);
            PlaneTexture = worldPositionMap;
            plane.InitializeResources(shadowMapResourseView, new Silver(), shadersPath, shadersPath, InputElements.VertexPosNormTex);
            plane.Rotation = Matrix.RotationYawPitchRoll(MathUtil.DegreesToRadians(0f), MathUtil.DegreesToRadians(-90f), MathUtil.DegreesToRadians(0f));
            plane.Translation = new Vector3(10f, 10f, 0f);

            consoleComponent = new ConsoleComponent();
            consoleComponent.Initialize(this);

            LuaManager= new LuaScriptManager(this);
          
        }

        public void DrawComponentsDeffered(float deltaTime)
        {
            DeviceContext.ClearDepthStencilView(dpv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 1);
            foreach (var renderTarget in DefferedRenderTargets)
            {
                DeviceContext.ClearRenderTargetView(renderTarget, Color.Black);
            }

            var eyepos = (Vector4)Camera.EyePosition;
            var location = Assembly.GetExecutingAssembly().Location;
            var shadersPath = Path.GetDirectoryName(location) + "\\Shaders\\DefferedSMShaders.hlsl";
            grid.ChangeShaders(shadersPath, shadersPath);
            cow.ChangeShaders(shadersPath, shadersPath);
            //grid.Draw(DeviceContext, DefferedRenderTargets, DepthStencilView, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
            //cow.Draw(DeviceContext, DefferedRenderTargets, DepthStencilView, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
            cow.Draw(DeviceContext, DefferedRenderTargets, dpv, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
            grid.Draw(DeviceContext, DefferedRenderTargets, dpv, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);

            
        }

        public override void DrawComponents(float deltaTi)
        {
            var eyepos = (Vector4)(Camera.EyePosition);
            var location = Assembly.GetExecutingAssembly().Location;
            var shadersPath = Path.GetDirectoryName(location) + "\\Shaders\\ShadowMapLightingShaders.hlsl";
            grid.ChangeShaders(shadersPath, shadersPath);
            cow.ChangeShaders(shadersPath, shadersPath);
            skySphere.ChangeShaders(shadersPath, shadersPath);
            skySphere.Draw(DeviceContext, forwardRenderTargets, DepthStencilView, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
            grid.Draw(DeviceContext, forwardRenderTargets, DepthStencilView, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
           // cow.Draw(DeviceContext, forwardRenderTargets, DepthStencilView, Camera.Proj, Camera.View, lightBufferStruct.Position, lightBufferStruct.Color, eyepos, lightBufferStruct.Intensity, PrimitiveTopology.TriangleList);
            // var normalmapView = new ShaderResourceView(GameDevice, shadowMap.DepthMap);
            var planeTextureView = new ShaderResourceView(GameDevice, PlaneTexture);
            plane.UpdateResources(planeTextureView);
            plane.Draw(DeviceContext, forwardRenderTargets, DepthStencilView, Camera.Proj, Camera.View, directionalLight.WorldPosition, directionalLight.Color, eyepos, directionalLight.Intensity, PrimitiveTopology.TriangleList); 

            if(consoleComponent.IsEnabled)
                consoleComponent.Draw();
        }

        public override void ShutdownGame()
        {
            Dispose();
        }

        private int k = 0;
        public void ChangeDefferedTarget()
        {
            k = k != DefferedTargetTextures.Length-1 ? k + 1 : 0;      
            PlaneTexture = DefferedTargetTextures[k]; 
        }

        public override void Draw(float deltaTime)
        {       
            DrawShadowMap();

            //Draw main components
            BindComponents();

            //DeviceContext.VertexShader.Set(VertexShader);
            //DeviceContext.PixelShader.Set(PixelShader);
            //DeviceContext.GeometryShader.Set(null);
            //var normalmapView = new ShaderResourceView(GameDevice, worldPositionMap);
            //DeviceContext.PixelShader.SetShaderResource(1, normalmapView);

            //var eyepos = new Vector4(Camera.EyePosition.X, Camera.EyePosition.Y, Camera.EyePosition.Z, 1f);

            //DeviceContext.UpdateSubresource(ref eyepos, eyeBuf);
            //DeviceContext.PixelShader.SetConstantBuffer(1, eyeBuf);

            //DeviceContext.UpdateSubresource(ref lightBufferStruct, lightBuf);
            //DeviceContext.PixelShader.SetConstantBuffer(2, lightBuf);
          //   DrawComponentsDeffered(deltaTime);

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
            DeviceContext.PixelShader.SetSampler(1, shadowMapSampler);
           // DeviceContext.PixelShader.SetShaderResource(1, shadowMapResourseView);

            DrawComponents(deltaTime);
            base.Draw(deltaTime);
        } 

        public override void InitializeBuffers()
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

            DepthStencilView = new DepthStencilView(GameDevice, depthBuffer);

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

        public void DrawShadowMap()
        {
            shadowMap.BindComponents(DeviceContext);
            grid.DrawShadowMap(DeviceContext, shadowMap.RenderTargetViews, shadowMap.DepthMapDsv, directionalLight.Projection, directionalLight.View, directionalLight.ShadowTransform, PrimitiveTopology.TriangleList);
            cow.DrawShadowMap(DeviceContext, shadowMap.RenderTargetViews, shadowMap.DepthMapDsv, directionalLight.Projection, directionalLight.View, directionalLight.ShadowTransform, PrimitiveTopology.TriangleList);
            //grid.DrawShadow(DeviceContext, directionalLight.ShadowTransform, directionalLight.Projection, directionalLight.View);
            //cow.DrawShadow(DeviceContext, directionalLight.ShadowTransform, directionalLight.Projection, directionalLight.View);
            // cobbl.DrawShadow(deviceContext, DirectionalLight.View, DirectionalLight.Projection);

            shadowMapResourseView = new ShaderResourceView(GameDevice, shadowMap.DepthMap);
        }
         
        public void BindComponents()
        {
            DeviceContext.Rasterizer.SetViewport(Viewport);
            DeviceContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);

         //   DeviceContext.InputAssembler.InputLayout = InputLayoutMain;
        }

        public override void KeyPressed(Keys key)
        {
            if(!consoleComponent.IsEnabled)
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
                    case Keys.E:
                        Camera.MoveUp();
                        break;
                    case Keys.Q:
                        Camera.MoveDown();
                        break;
                    case Keys.Escape:
                        ShutdownGame();
                        break;
                    case Keys.F:
                        ChangeDefferedTarget();
                        break;
                    case Keys.Oem3:
                        consoleComponent.ToogleVisibility();
                        break;
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.Oem3:
                        consoleComponent.ToogleVisibility();
                        break;
                    default:
                        consoleComponent.ConsumeKey(key);
                        break;
                }
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
            DepthStencilView.Dispose();
            foreach (var renderTarget in DefferedRenderTargets)
            {
                renderTarget.Dispose();
            }
        }

        public override void MouseMoved(float x, float y)
        {
            if(!consoleComponent.IsEnabled)
                Camera.ChangeTargetPosition(x, y);
        }
    }
}