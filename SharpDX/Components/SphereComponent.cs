using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;

namespace SharpDX.Components
{
    class SphereComponent : AbstractComponent
    {
        private Stopwatch clock = new Stopwatch();

        private int[] indices;

        //private VertexPositionColorTexture[] vertices;

        private Direct3D11.Buffer indexBuffer;

        public float Diametr = 0.3f;
        public float Radius;

        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
            new Direct3D11.InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
        };

        public SphereComponent(Direct3D11.Device device, float diametr = 1.0f, int tessellation = 16)
        {
            this.device = device;
            WorldPosition = new Vector3(0f, 0f, 0f);
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = Matrix.Translation(new Vector3(0f, 0f, 0f));
            Scaling = Matrix.Scaling(1);

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            //vert = new VertexPositionColorTexture[((verticalSegments + 1) * (horizontalSegments + 1))];
            vertices = new Vector4[((verticalSegments + 1) * (horizontalSegments + 1))*2];
            indices = new int[verticalSegments * (horizontalSegments + 1) * 6];

            Diametr = diametr;
            Radius = diametr / 2;

            int vertCount = 0;
            for (int i = 0; i <= verticalSegments; i++)
            {
                var lat = (float)((i * Math.PI / verticalSegments) - Math.PI / 2);

                var CosX = (float)Math.Cos(lat);
                var y = Radius * (float)Math.Sin(lat);
                var CosZ = (float)Math.Cos(lat);

                float v = 1.0f - (float)i / verticalSegments;

                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;

                    var lon = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var x = Radius * CosX * (float)Math.Sin(lon);
                    var z = Radius * CosZ * (float)Math.Cos(lon);

                    vertices[vertCount++] = new Vector4(x, y, z, 1.0f);
                    vertices[vertCount++] = Color.Red.ToVector4();
                    //vert[vertCount++] = new VertexPositionColorTexture(new Vector3(x, y, z), Color.Red.ToColor4(), new Vector2(u, v));
                }
            }


            int stride = horizontalSegments + 1;

            int indexCount = 0;
            for (int i = 0; i < verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % stride;

                    indices[indexCount++] = (i * stride + j);
                    indices[indexCount++] = (nextI * stride + j);
                    indices[indexCount++] = (i * stride + nextJ);

                    indices[indexCount++] = (i * stride + nextJ);
                    indices[indexCount++] = (nextI * stride + j);
                    indices[indexCount++] = (nextI * stride + nextJ);
                }
            }


            vertexBuffer = Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);

            indexBuffer = Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            clock.Start();
        }

     

        public override void Draw(DeviceContext deviceContext, Matrix proj, Direct3D11.Buffer initialConstantBuffer)
        {
            //ShaderSignature inputSignature;
            //VertexShader vertexShader;
            //PixelShader pixelShader;

            //using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("ShaderTextures.fx", "VS", "vs_5_0", ShaderFlags.Debug))
            //{
            //    inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            //    vertexShader = new Direct3D11.VertexShader(deviceContext.Device, vertexShaderByteCode);
            //}

            //using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("ShaderTextures.fx", "PS", "ps_5_0", ShaderFlags.Debug))
            //{
            //    pixelShader = new Direct3D11.PixelShader(deviceContext.Device, pixelShaderByteCode);
            //}

            //deviceContext.VertexShader.Set(vertexShader);
            //deviceContext.PixelShader.Set(pixelShader);


            //var inputLayout = new InputLayout(deviceContext.Device, inputSignature, inputElements);
            //deviceContext.InputAssembler.InputLayout = inputLayout;

            var time = clock.ElapsedMilliseconds / 1000f;
            Translation = Matrix.Translation((float)Math.Cos(time / 2), 0, (float)Math.Sin(time / 2));

            var worldViewProj = Rotation * Translation * Scaling * proj;

            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32G32B32A32_Float, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>()*2, 0));

            deviceContext.Draw(vertices.Count(), 0);

            deviceContext.VertexShader.SetConstantBuffer(0, initialConstantBuffer);
        }

        public override void Update()
        {
            vertices = Transformation(vertices, WorldPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
        }

        public Vector4[] Transformation(Vector4[] vertices, Vector3 translation, Matrix rotation)
        {
            Vector4[] ret = new Vector4[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0f, 0f, 0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i += 2)
            {
                ret[i] = Vector4.Transform(vertices[i], transform);
                ret[i + 1] = vertices[i + 1];
            }

            return ret;
        }

    }
}