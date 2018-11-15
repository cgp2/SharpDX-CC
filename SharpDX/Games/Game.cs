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
using System.IO;
using SharpDX.RawInput;
using SharpDX.Multimedia;
using System.Windows.Forms;


namespace SharpDX
{
    public abstract class Game 
    {
        public Direct3D11.Device GameDevice;
        public RenderForm RenderForm;
        public CameraComponent Camera;
        public static ObjLoader ObjLoader;

        public Direct3D11.DeviceContext DeviceContext;
        protected RenderTargetView RenderTargetView;
        protected Direct3D11.VertexShader VertexShader;
        protected InputDevice InputDevice;
        protected SwapChain SwapChain;

        protected const int Width = 800;
        protected const int Height = 800;

        protected Direct3D11.Buffer VertexBuffer;
        protected Direct3D11.PixelShader PixelShader;
        protected Viewport Viewport;
        protected ShaderSignature InputSignature;
        protected Direct3D11.InputLayout InputLayoutMain;
        protected Texture2D depthBuffer;
        protected DepthStencilView DepthStencilView;
        protected DXGI.Factory Factory;
        public Texture2D BackBufer;

        protected SwapChainDescription SwapChainDescriptor;

        private readonly Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0)
        };

        public bool IsActive => true;
        protected Stopwatch FrameTimer;
        private float tempTime;
        private float deltaTime;

        protected List<Components.AbstractComponent> Components = new List<Components.AbstractComponent>();

        protected Game()
        {
            RenderForm = new RenderForm("Default3D")
            {
                ClientSize = new Size(Width, Height),
                AllowUserResizing = false
            };
            InitializeDeviceResources();

            ObjLoader = new ObjLoader();
            
            InputDevice = new InputDevice(this);
        }

        protected void InitializeDeviceResources()
        {
            var backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            SwapChainDescriptor = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = backBufferDesc,
                IsWindowed = true,
                OutputHandle = RenderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, Direct3D11.DeviceCreationFlags.BgraSupport, SwapChainDescriptor, out GameDevice, out SwapChain);
            DeviceContext = GameDevice.ImmediateContext;

            Viewport = new Viewport(0, 0, Width, Height, 0.0f, 1.0f);
            DeviceContext.Rasterizer.SetViewport(Viewport);

            Factory = SwapChain.GetParent<SharpDX.DXGI.Factory>();
            //factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            BackBufer = SwapChain.GetBackBuffer<Direct3D11.Texture2D>(0);
            RenderTargetView = new RenderTargetView(GameDevice, BackBufer);
        }

        public abstract void InitializeLight();
        public abstract void InitializeComponents();
        public abstract void InitializeBuffers();
        public abstract void DrawComponents(float deltaTime);

        public abstract void ShutdownGame();

        public virtual void RenderCallback()
        {
            DeviceContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            DeviceContext.ClearRenderTargetView(RenderTargetView, Color.Black);

            deltaTime = FrameTimer.ElapsedMilliseconds;
            Draw(deltaTime - tempTime);
            tempTime = deltaTime;
        }

        public virtual void Run()
        {
            InitializeBuffers();
            InitializeLight();
            InitializeComponents();

            FrameTimer = new Stopwatch();
            FrameTimer.Start();
            RenderLoop.Run(RenderForm, RenderCallback);
        }

        public virtual void Draw(float deltaTime)
        {
            SwapChain.Present(1, PresentFlags.None);
        }

        protected void InitializeShaders()
        { 
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_4_0", ShaderFlags.Debug))
            {
                InputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertexShader = new Direct3D11.VertexShader(GameDevice, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_4_0", ShaderFlags.Debug))
            {
                PixelShader = new Direct3D11.PixelShader(GameDevice, pixelShaderByteCode);
            }

            DeviceContext.VertexShader.Set(VertexShader);
            DeviceContext.PixelShader.Set(PixelShader);

            InputLayoutMain = new InputLayout(GameDevice, InputSignature, inputElements);
            DeviceContext.InputAssembler.InputLayout = InputLayoutMain;
        }

        public abstract void KeyPressed(Keys key);

        public abstract void MouseMoved(float x, float y);

        protected struct LightBufferStruct
        {
            public Vector4 Position;
            public Vector4 Color;
            public Vector4 EyePos;
            public float Intensity;
            public float Dum1, Dum2, Dum3;
        }

        public void DisposeBase()
        {
            BackBufer.Dispose();
            RenderForm.Dispose();
            RenderTargetView.Dispose();
            GameDevice.Dispose();
            SwapChain.Dispose();
            DeviceContext.Dispose();
            //InputLayoutMain.Dispose();
           // InputSignature.Dispose();
        }
    }
}
