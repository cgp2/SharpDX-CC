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
        public Direct3D11.Device device;
        public RenderForm renderForm;
        public CameraComponent Camera;

        public Direct3D11.DeviceContext deviceContext;
        protected RenderTargetView renderTargetView;
        protected Direct3D11.VertexShader vertexShader;
        protected InputDevice inp;
        protected SwapChain swapChain;

        protected const int width = 800;
        protected const int height = 800;

        protected Direct3D11.Buffer vertexBuffer;
        protected Direct3D11.PixelShader pixelShader;
        protected Viewport viewport;
        protected ShaderSignature inputSignature;
        protected Direct3D11.InputLayout inputLayout;

        protected SwapChainDescription swapChainDescriptor;

        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0)
        };

        public bool IsActive { get { return true; } }

        protected List<Components.AbstractComponent> components = new List<Components.AbstractComponent>();

        public Game()
        {
            renderForm = new RenderForm("Default3D");
            renderForm.ClientSize = new Size(width, height);
            renderForm.AllowUserResizing = false;
            InitializeDeviceResources();

            inp = new InputDevice(this);
        }

        protected void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            swapChainDescriptor = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = backBufferDesc,
                IsWindowed = true,
                OutputHandle = renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, Direct3D11.DeviceCreationFlags.None, swapChainDescriptor, out device, out swapChain);
            deviceContext = device.ImmediateContext;
            
            viewport = new Viewport(0, 0, width, height, 0.0f, 1.0f);
            deviceContext.Rasterizer.SetViewport(viewport);

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            using (Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<Direct3D11.Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, backBuffer);
            }
        }

        protected void InitializeDeviceResources(Direct3D11.Device dev)
        {
            device = dev;
            deviceContext = device.ImmediateContext;

            viewport = new Viewport(0, 0, width, height, 0.0f, 1.0f);
            deviceContext.Rasterizer.SetViewport(viewport);

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            using (Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<Direct3D11.Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, backBuffer);
            }
        }

        protected void InitializeShaders()
        { 
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);

            inputLayout = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayout;
        }

        public abstract void KeyPressed(Keys key);

        public abstract void MouseMoved(float x, float y);

        public void DisposeBase()
        {
            renderForm.Dispose();
            renderTargetView.Dispose();
            device.Dispose();
            swapChain.Dispose();
            deviceContext.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();
        }
    }
}
