using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDX.Components
{
    internal class SoComponent : IDisposable
    {
        private readonly Buffer bufferForSo;

        private readonly InputElement[] colorInputElements =
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
        };

        private readonly InputLayout colorInputLayout;
        private readonly ShaderSignature colorInputSignature;

        private readonly PixelShader colorPixelShader;
        private readonly VertexShader colorVertexShader;

        private readonly Buffer constantBufferColor;

        private readonly GeometryShader geometryShader;

        private readonly Buffer soBuffer;

        private readonly InputElement[] soInputElement =
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
        };

        private readonly InputLayout soInputLayout;
        private readonly ShaderSignature soInputSignature;
        private readonly VertexShader vertexShaderSo;

        private readonly VertexPositionNormalTexture[] vertices;

        public SoComponent(Device device, VertexPositionNormalTexture[] vertices, string geometryShaderName, string pixelShaderName)
        {
            this.vertices = vertices;
            bufferForSo = Buffer.Create(device, BindFlags.VertexBuffer, this.vertices);

            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location) + "\\Shaders\\" + pixelShaderName;

            using (var vertexShaderByteCode =
                ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                colorInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                colorVertexShader = new VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode =
                ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                colorPixelShader = new PixelShader(device, pixelShaderByteCode);
            }

            path = Path.GetDirectoryName(location) + "\\Shaders\\" + geometryShaderName;
            using (var geometryShaderByteCode = ShaderBytecode.CompileFromFile(path, "GS", "gs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                StreamOutputElement[] streamOutput =
                {
                    new StreamOutputElement
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        StartComponent = 0,
                        ComponentCount = 4,
                        OutputSlot = 0
                    },

                    new StreamOutputElement
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        StartComponent = 0,
                        ComponentCount = 4,
                        OutputSlot = 0
                    }
                };

                int[] bufferStride =
                {
                    Utilities.SizeOf<Vector4>() + Utilities.SizeOf<Color4>()
                };

                geometryShader = new GeometryShader(device, geometryShaderByteCode, streamOutput, bufferStride, -1);
            }

            using (var vertexShaderByteCode =
                ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                soInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShaderSo = new VertexShader(device, vertexShaderByteCode);
            }

            colorInputLayout = new InputLayout(device, colorInputSignature, colorInputElements);
            soInputLayout = new InputLayout(device, soInputSignature, soInputElement);

            constantBufferColor = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            soBuffer = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                StructureByteStride = Utilities.SizeOf<VertexPositionColor>(),
                SizeInBytes = Utilities.SizeOf<VertexPositionColor>() * Utilities.SizeOf(vertices)
            });
        }

        public void Dispose()
        {
            geometryShader.Dispose();
            vertexShaderSo.Dispose();
            soInputSignature.Dispose();
            soInputLayout.Dispose();
            colorPixelShader.Dispose();
            colorVertexShader.Dispose();
            colorInputSignature.Dispose();
            colorInputLayout.Dispose();
            constantBufferColor.Dispose();
            bufferForSo.Dispose();
            soBuffer.Dispose();
        }

        public void BindComponentsSoStage(DeviceContext deviceContext)
        {
            deviceContext.StreamOutput.SetTarget(soBuffer, 0);

            deviceContext.VertexShader.Set(vertexShaderSo);
            deviceContext.PixelShader.Set(null);
            deviceContext.GeometryShader.Set(geometryShader);
            deviceContext.InputAssembler.InputLayout = soInputLayout;
        }

        public void BindComponentsPsStage(DeviceContext deviceContext)
        {
            deviceContext.StreamOutput.SetTargets(null);

            deviceContext.VertexShader.Set(colorVertexShader);
            deviceContext.PixelShader.Set(colorPixelShader);
            deviceContext.InputAssembler.InputLayout = colorInputLayout;

            deviceContext.DrawAuto();
        }

        public void DrawSo(DeviceContext deviceContext)
        {
            BindComponentsSoStage(deviceContext);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(bufferForSo, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            deviceContext.Draw(vertices.Count(), 0);
        }

        public void DrawPs(DeviceContext deviceContext, Matrix view, Matrix proj, Matrix transform)
        {
            BindComponentsPsStage(deviceContext);

            var viewProj = transform * view * proj;
            deviceContext.UpdateSubresource(ref viewProj, constantBufferColor, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, constantBufferColor);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(soBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

            deviceContext.DrawAuto();
        }
    }
}