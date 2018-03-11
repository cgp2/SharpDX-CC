using System;
using System.Diagnostics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDX.Components
{
    internal class SphereComponent : AbstractComponent
    {
        private readonly Stopwatch clock = new Stopwatch();

        public float Diametr = 0.3f;

        //private VertexPositionColorTexture[] InitialPoints;

        private readonly Buffer indexBuffer;

        private readonly int[] indices;
        public Vector4 Position;
        public float Radius;

        public SphereComponent(Device device, float diametr = 1.0f, int tessellation = 16)
        {
            Device = device;

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            Position = new Vector4(InitialPosition, 1);
            var verticalSegments = tessellation;
            var horizontalSegments = tessellation * 2;

            Vertices = new VertexPositionNormalTexture[(verticalSegments + 1) * (horizontalSegments + 1)];
            indices = new int[verticalSegments * (horizontalSegments + 1) * 6];

            Diametr = diametr;
            Radius = diametr / 2;

            var vertCount = 0;
            for (var i = 0; i <= verticalSegments; i++)
            {
                var lat = (float) (i * Math.PI / verticalSegments - Math.PI / 2);

                var cosX = (float) Math.Cos(lat);
                var y = (float) Math.Sin(lat);
                var cosZ = (float) Math.Cos(lat);

                var v = 1.0f - (float) i / verticalSegments;

                for (var j = 0; j <= horizontalSegments; j++)
                {
                    var u = (float) j / horizontalSegments;

                    var lon = (float) (j * 2.0 * Math.PI / horizontalSegments);
                    var x = cosX * (float) Math.Sin(lon);
                    var z = cosZ * (float) Math.Cos(lon);

                    Vertices[vertCount].Position = new Vector4(x, y, z, 1.0f) * Radius;
                    Vertices[vertCount].Normal = new Vector4(x, y, z, 0);
                    Vertices[vertCount].Texture = new Vector2(u, v);
                    vertCount++;
                    //vert[vertCount++] = new VertexPositionColorTexture(new Vector3(x, y, z), Color.Red.ToColor4(), new Vector2(u, v));
                }
            }

            var stride = horizontalSegments + 1;

            var indexCount = 0;
            for (var i = 0; i < verticalSegments; i++)
            for (var j = 0; j <= horizontalSegments; j++)
            {
                var nextI = i + 1;
                var nextJ = (j + 1) % stride;

                indices[indexCount++] = i * stride + j;
                indices[indexCount++] = nextI * stride + j;
                indices[indexCount++] = i * stride + nextJ;

                indices[indexCount++] = i * stride + nextJ;
                indices[indexCount++] = nextI * stride + j;
                indices[indexCount++] = nextI * stride + nextJ;
            }

            Texture = TextureLoader.CreateTexture2DFromBitmap(device,
                TextureLoader.LoadBitmap(new ImagingFactory2(), "Transparent.png"));
            TextureView = new ShaderResourceView(device, Texture);
            SamplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear
            };
            Sampler = new SamplerState(device, SamplerStateDescription);

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Vertices);

            indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

            ConstantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            clock.Start();
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        {
            var time = clock.ElapsedMilliseconds / 1000f;

            Transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter,
                Quaternion.RotationMatrix(Rotation), Translation);
            WorldPosition = Vector3.Transform(InitialPosition, Transform);
            var worldViewProj = Transform * view * proj;

            deviceContext.PixelShader.SetShaderResource(0, TextureView);
            deviceContext.PixelShader.SetSampler(0, Sampler);
            deviceContext.UpdateSubresource(ref worldViewProj, ConstantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, ConstantBuffer);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32G32B32A32_Float, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            //deviceContext.Draw(Vertices.Count(), 0);
        }

        public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView)
        {
            throw new NotImplementedException();
        }

        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation,
            Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];
            var transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity,
                new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation),
                translation);

            for (var i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, transform);
                ret[i].Normal = vertices[i].Normal;
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public override void Update()
        {
            Vertices = Transformation(Vertices, InitialPosition, Rotation);
            VertexBuffer = Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
            Sampler.Dispose();
            Texture.Dispose();
            TextureView.Dispose();
            ConstantBuffer.Dispose();
        }
    }
}