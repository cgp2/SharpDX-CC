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

namespace SharpDX
{
    class ShadowMap : IDisposable
    {
        //public Texture2D shadowMap;
        public RenderTargetView[] RenderTargetViews;
        public DepthStencilView DepthMapDsv;
        private Direct3D11.Device device;
        private readonly Viewport viewport;
        private readonly int width;
        private readonly int height;
        public Texture2D DepthMap = null;



        public Texture2D DsvText;

        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        };

        public ShadowMap(Direct3D11.Device device, SwapChain swapChain, int width, int height)
        {
            this.device = device;
            this.width = width;
            this.height = height;

            viewport = new Viewport(0, 0, width, height, 0, 1f);

            //shadowMap = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

            var depthDesc = new Texture2DDescription
            {
                Width = this.width,
                Height = this.height,
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

            DepthMap = new Texture2D(device, depthDesc);

            RenderTargetViews = new []
            {
               new RenderTargetView(device, DepthMap)
            };

            DsvText = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            DepthMapDsv = new DepthStencilView(device, DsvText);

            //var shaderResourseViewDesc = new ShaderResourceViewDescription
            //{
            //    Format = Format.D32_Float,
            //    Dimension = ShaderResourceViewDimension.Texture2D,
            //    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
            //    {
            //        MipLevels = 1,
            //        MostDetailedMip = 0,
            //    }
            //};
            //shaderResourceView = new ShaderResourceView(GameDevice, DSVText, shaderResourseViewDesc);

            //var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //var path = Path.GetDirectoryName(location) + "\\Shaders\\ShadowMapShaders.hlsl";
            //using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            //{
            //    shadowInputShaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            //    shadowVertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            //}

            //using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            //{
            //    shadowPixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            //}

            //shadowInputLayout = new InputLayout(device, shadowInputShaderSignature, inputElements);
        }

        public void BindComponents(DeviceContext deviceContext)
        {
            deviceContext.Rasterizer.SetViewport(viewport);
           // deviceContext.OutputMerger.SetRenderTargets(RenderTargetView);
            deviceContext.ClearDepthStencilView(DepthMapDsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);
            deviceContext.ClearRenderTargetView(RenderTargetViews[0], Color.White);
        }

        public void Dispose()
        {
            
            DepthMapDsv.Dispose();
            foreach (var renderTarget in RenderTargetViews)
            {
               renderTarget.Dispose();
            }      
        }
    }
}
