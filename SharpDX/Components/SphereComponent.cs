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

        Vector2[] textCoords;
        Texture2D texture;
        ShaderResourceView textureView;
        SamplerStateDescription samplerStateDescription;
        SamplerState sampler;
        public Vector4 position;

        public SphereComponent(Direct3D11.Device device, float diametr = 1.0f, int tessellation = 16)
        {
            this.device = device;
           
            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            position = new Vector4(InitialPosition, 1);
            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            t = new VertexPositionNormalTexture[((verticalSegments + 1) * (horizontalSegments + 1))];
            indices = new int[verticalSegments * (horizontalSegments + 1) * 6];

            Diametr = diametr;
            Radius = diametr / 2;

            int vertCount = 0;
            for (int i = 0; i <= verticalSegments; i++)
            {
                var lat = (float)((i * Math.PI / verticalSegments) - Math.PI / 2);

                var CosX = (float)Math.Cos(lat);
                var y = (float)Math.Sin(lat);
                var CosZ = (float)Math.Cos(lat);

                float v = 1.0f - (float)i / verticalSegments;

                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;

                    var lon = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var x = CosX * (float)Math.Sin(lon);
                    var z = CosZ * (float)Math.Cos(lon);

                    t[vertCount].Position = new Vector4(x, y, z, 1.0f) * Radius;
                    t[vertCount].Normal = new Vector4(x, y, z, 0);
                    t[vertCount].Texture = new Vector2(u, v);
                    vertCount++;
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

            texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "Transparent.png"));
            textureView = new ShaderResourceView(device, texture);
            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear
            };
            sampler = new SamplerState(device, samplerStateDescription);

            vertexBuffer = Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, t);

            indexBuffer = Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            clock.Start();
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, bool toStreamOutput)
        { 
            var time = clock.ElapsedMilliseconds / 1000f;

            transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            WorldPosition = Vector3.Transform(InitialPosition, transform);
            var worldViewProj = transform * view * proj;

            deviceContext.PixelShader.SetShaderResource(0, textureView);
            deviceContext.PixelShader.SetSampler(0, sampler);
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32G32B32A32_Float, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            //deviceContext.Draw(t.Count(), 0);
        }

        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, transform);
                ret[i].Normal = vertices[i].Normal;
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public override void Update()
        {
            t = Transformation(t, InitialPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            sampler.Dispose();
            texture.Dispose();
            textureView.Dispose();
            constantBuffer.Dispose();
        }
    }
}