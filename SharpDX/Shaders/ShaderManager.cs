using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Materials;
using SharpDX.Direct3D;
using SharpDX.InputStructures;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDX
{
    public class ShaderManager : IDisposable
    {
        public VertexShader VertShader;
        public PixelShader PixShader;
        public InputElement[] InputElem;
        public InputLayout Layout;

        public Direct3D11.Buffer LightBuffer;
        public Direct3D11.Buffer MatrixBuffer;
        public Direct3D11.Buffer MaterialBuffer;
        public Direct3D11.Buffer VertexBuffer;
        public Direct3D11.Buffer FlagsBuffer;

        public MatrixBuf MatrixRes;
        public LightBuf LightRes;
        public MaterialBuf MaterialRes;
        public FlagsBuf FlagsRes;

        public int InputTypeSize;

        public SamplerState Sampler;

        private Device device;

        public struct MatrixBuf
        {
            public Matrix World;
            public Matrix View;
            public Matrix Proj;
        }

        public struct LightBuf
        {
            public Vector4 Position;
            public Vector4 Color;
            public Vector4 EyePos;
            public float Intensity;
            private float dum1, dum2, dum3;
        }

        public struct MaterialBuf
        {
            public Vector4 Diffuse;
            public Vector4 Absorption;
            public Vector4 Ambient;
            public float Shiness;
            private float dum1, dum2, dum3;
        }

        public struct FlagsBuf
        {
            public float IsTextureCube;
            public float IsLighten;
            private float dum1, dum2;
        }

        protected Direct3D11.Buffer ShadowTransformBuffer;

        public Matrix ShadowTransform;

        public bool Initialize(Device dev, InputElements inputType, Direct3D11.Buffer vertBuffer, string pathVertex, string pathPixel)
        {
            device = dev;
            VertexBuffer = vertBuffer;

            switch (inputType)
            {
                case InputElements.VertexPosCol:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<VertexPositionColor>();
                    break;
                case InputElements.VertexPosColTex:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<VertexPositionColorTexture>();
                    break;
                case InputElements.VertexPosNormTex:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<VertexPositionNormalTexture>();
                    break;
                case InputElements.VertexPosNormWorldPos:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
                        new InputElement("SV_POSITION", 0, Format.R32G32_Float, 48, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<VertexPositionNormalWorldPosTexture>();
                    break;
                case InputElements.TrgPosNormTex:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<TriangleInput>();
                    break;
                case InputElements.TrgAdjPosNormTex:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<TrianglePositionNormalTextureAdjInput>();
                    break;
                case InputElements.TrgStripPosNormTex:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
                    };
                    InputTypeSize = Utilities.SizeOf<TrianglePositionNormalTextureStripInput>();
                    break;
                case InputElements.ShadowMapInput:
                    InputElem = new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    };
                    InputTypeSize = Utilities.SizeOf<Vector4>();
                    break;
            }

            InitializeShaders(pathVertex, pathPixel);

            return true;
        }
        //Вынести в наследника
        public bool Initialize(Device dev, Direct3D11.Buffer vertBuffer, bool isShadowMap)
        {
            device = dev;
            VertexBuffer = vertBuffer;

            InputElem = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            };
            InputTypeSize = Utilities.SizeOf<VertexPositionNormalTexture>();

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location) + "\\Shaders\\ShadowMapShaders.hlsl";
            InitializeShaders(path, path);

            return true;
        }

        public void ChangeShaders(string pathVertShader, string pathPixShader)
        {
            PixShader.Dispose();
            VertShader.Dispose();

            ShaderSignature inputSignature;
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(pathVertShader, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertShader = new VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(pathPixShader, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                PixShader = new PixelShader(device, pixelShaderByteCode);
            }

            Layout = new InputLayout(device, inputSignature, InputElem);
        }

        protected void InitializeShaders(string pathVert, string pathPixel)
        {
            ShaderSignature inputSignature;
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(pathVert, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertShader = new VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(pathPixel, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                PixShader = new PixelShader(device, pixelShaderByteCode);
            }

            Layout = new InputLayout(device, inputSignature, InputElem);

            LightBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<LightBuf>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MatrixBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<MatrixBuf>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MaterialBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<MaterialBuf>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowTransformBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            FlagsBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<FlagsBuf>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear
            };
            Sampler = new SamplerState(device, samplerStateDescription);
        }

        private void SetShadowMapParameters(DeviceContext deviceContext, Matrix transform)
        {
            var shdTr = transform * ShadowTransform;

            deviceContext.UpdateSubresource(ref shdTr, ShadowTransformBuffer);
            deviceContext.VertexShader.SetConstantBuffer(3, ShadowTransformBuffer);
        }


        public void Render(DeviceContext deviceContext, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilView,
                           Matrix view, Matrix proj, Matrix world,
                           Vector4 lightPos, Vector4 lightCol, Vector4 eyePos, float lightIntens,
                           AbstractMaterial mat,
                           ShaderResourceView texture, bool isTextureCube, bool isLighten,
                           PrimitiveTopology topology,
                           int vertexCount, bool withShadowMap = true)
        {

            SetParameters(deviceContext, view, proj, world, lightPos, lightCol, eyePos, lightIntens, mat, texture, isTextureCube, isLighten);

            if (withShadowMap)
            {
                SetShadowMapParameters(deviceContext, world);
            }

            RenderShader(deviceContext, renderTargetViews, depthStencilView, topology, vertexCount);
        }

        public void RenderShadow(DeviceContext deviceContext, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilView,
                                 Matrix lightView, Matrix lightProj, Matrix shadowTransform, Matrix world,
                                 PrimitiveTopology topology, int vertexCount)
        {
            SetShadowParameters(deviceContext, lightView, lightProj, shadowTransform, world, vertexCount);

            RenderShader(deviceContext, renderTargetViews, depthStencilView, topology, vertexCount);
        }

        protected void SetShadowParameters(DeviceContext deviceContext, Matrix lightView, Matrix lightProj, Matrix shadowTransform, Matrix transform, int vertexCount)
        {

            MatrixRes = new MatrixBuf()
            {
                World = transform,
                View = lightView,
                Proj = lightProj,
            };

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, InputTypeSize, 0));

            deviceContext.UpdateSubresource(ref MatrixRes, MatrixBuffer);
            deviceContext.VertexShader.SetConstantBuffer(0, MatrixBuffer);
        }

        protected void SetParameters(DeviceContext deviceContext, Matrix view, Matrix proj, Matrix world,
                                    Vector4 lightPos, Vector4 lightCol, Vector4 eyePos, float lightIntensity,
                                    AbstractMaterial mat,
                                    ShaderResourceView texture, bool isTextureCube, bool isLighten)
        {
            MatrixRes = new MatrixBuf()
            {
                World = world,
                View = view,
                Proj = proj,
            };

            LightRes = new LightBuf()
            {
                Position = lightPos,
                Color = lightCol,
                EyePos = eyePos,
                Intensity = lightIntensity
            };

            MaterialRes = new MaterialBuf()
            {
                Absorption = new Vector4(mat.Absorption, 0),
                Ambient = new Vector4(mat.Ambient, 0),
                Diffuse = new Vector4(mat.Diffuse, 0),
                Shiness = mat.Shiness
            };

            FlagsRes = new FlagsBuf()
            {
                IsTextureCube = isTextureCube ? 1.0f : 0.0f,
                IsLighten = isLighten ? 1.0f : 0.0f
            };

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, InputTypeSize, 0));

            deviceContext.UpdateSubresource(ref MatrixRes, MatrixBuffer);
            deviceContext.VertexShader.SetConstantBuffer(0, MatrixBuffer);

            deviceContext.UpdateSubresource(ref LightRes, LightBuffer);
            deviceContext.PixelShader.SetConstantBuffer(1, LightBuffer);

            deviceContext.UpdateSubresource(ref MaterialRes, MaterialBuffer);
            deviceContext.PixelShader.SetConstantBuffer(2, MaterialBuffer);

            deviceContext.UpdateSubresource(ref FlagsRes, FlagsBuffer);
            deviceContext.PixelShader.SetConstantBuffer(4, FlagsBuffer);

            if (!isTextureCube)
            {
                deviceContext.PixelShader.SetShaderResource(0, texture);
            }
            else
            {
                deviceContext.PixelShader.SetShaderResource(2, texture);

            }
            deviceContext.PixelShader.SetSampler(0, Sampler);
            //ShadowBuf

        }

        private void RenderShader(DeviceContext deviceContext, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilView, PrimitiveTopology topology, int indexCount)
        {
            deviceContext.OutputMerger.ResetTargets();
            deviceContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetViews);
            deviceContext.InputAssembler.InputLayout = Layout;

            deviceContext.VertexShader.Set(VertShader);
            deviceContext.PixelShader.Set(PixShader);

            deviceContext.InputAssembler.PrimitiveTopology = topology;

            deviceContext.Draw(indexCount, 0);
        }

        public void Dispose()
        {
            LightBuffer.Dispose();
            MatrixBuffer.Dispose();
            VertexBuffer.Dispose();
            MaterialBuffer.Dispose();
            VertShader.Dispose();
            PixShader.Dispose();
            Layout.Dispose();
            Sampler.Dispose();
        }
    }
}
