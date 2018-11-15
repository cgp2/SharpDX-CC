using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.InputStructures;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDX.Components
{
    class ShadowVolumeComponent : IDisposable
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

        private Buffer lightPosBuffer;

        private readonly TrianglePositionNormalTextureAdjInput[] vertices;

        private Buffer indexBuffer;
        private int[] indices;

        public ShadowVolumeComponent(Device device, TrianglePositionNormalTextureAdjInput[] vertices, string geometryShaderName, string pixelShaderName)
        {
            this.vertices = vertices;
            bufferForSo = Buffer.Create(device, BindFlags.VertexBuffer, this.vertices);

            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location) + "\\Shaders\\" + pixelShaderName;

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                colorInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                colorVertexShader = new VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
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
                    Utilities.SizeOf<Vector4>()*4 + Utilities.SizeOf<Color4>()*4
                };

                geometryShader = new GeometryShader(device, geometryShaderByteCode, streamOutput, bufferStride, -1);
                lightPosBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Vector4>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

                //indexBuffer = Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);
            }

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                soInputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShaderSo = new VertexShader(device, vertexShaderByteCode);
            }

            colorInputLayout = new InputLayout(device, colorInputSignature, colorInputElements);
            soInputLayout = new InputLayout(device, soInputSignature, soInputElement);

            constantBufferColor = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            soBuffer = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                StructureByteStride = Utilities.SizeOf<VertexPositionColor>(),
                SizeInBytes = Utilities.SizeOf<VertexPositionColor>() * 10000
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

        public void BindComponentsSOStage(DeviceContext deviceContext)
        {
            deviceContext.StreamOutput.SetTarget(soBuffer, 0);

            deviceContext.VertexShader.Set(vertexShaderSo);
            deviceContext.PixelShader.Set(null);
            deviceContext.GeometryShader.Set(geometryShader);
            deviceContext.InputAssembler.InputLayout = soInputLayout;
        }

        public void BindComponentsPSStage(DeviceContext deviceContext)
        {
            deviceContext.StreamOutput.SetTargets(null);

            deviceContext.VertexShader.Set(colorVertexShader);
            deviceContext.PixelShader.Set(colorPixelShader);
            deviceContext.InputAssembler.InputLayout = colorInputLayout;

            deviceContext.DrawAuto();
        }

        public void DrawSO(DeviceContext deviceContext, Vector4 lightPos)
        {
            BindComponentsSOStage(deviceContext);

            deviceContext.UpdateSubresource(ref lightPos, lightPosBuffer);
            deviceContext.GeometryShader.SetConstantBuffer(0, lightPosBuffer);


            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(bufferForSo, Utilities.SizeOf<TrianglePositionNormalTextureAdjInput>(), 0));
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleListWithAdjacency;

            deviceContext.Draw(vertices.Count(), 0);
        }

        public void DrawPS(DeviceContext deviceContext, Matrix view, Matrix proj, Matrix transform)
        {
            BindComponentsPSStage(deviceContext);

            var viewProj = transform * view * proj;
            deviceContext.UpdateSubresource(ref viewProj, constantBufferColor, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, constantBufferColor);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(soBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

            deviceContext.DrawAuto();
        }
    }
}

