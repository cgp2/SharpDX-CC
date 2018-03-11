using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace SharpDX.Components
{
    internal class PlaneComponent : AbstractComponent
    {
        public PlaneComponent(Device device, ShaderResourceView text)
        {
            Device = device;

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            InitialPoints = new[]
            {
                new Vector4(-4.0f, 0.0f, -4.0f,  1.0f), 
                new Vector4(-4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4(-4.0f, 0.0f, -4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f, -4.0f,  1.0f),
            };

            var textCoords = new[]
            {
                new Vector2(0f, 1f), 
                new Vector2(0f, 0f),
                new Vector2(1, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),

            };

            var normals = new[]
            {
                new Vector4(-4.0f, 1.0f, -4.0f, 0.0f),
                new Vector4(-4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4(-4.0f, 1.0f, -4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f, -4.0f, 0.0f),
            };

            VerticesCount = InitialPoints.Length;
            Vertices = new VertexPositionNormalTexture[VerticesCount];
            for(var i = 0; i < VerticesCount; i++)
            {
                Vertices[i].Position = InitialPoints[i];
                Vertices[i].Normal = normals[i];
                Vertices[i].Texture = textCoords[i];
            }

            InitialVertices = Vertices;

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Vertices);

            TextureView = text;

            SamplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear,
            };
            Sampler = new SamplerState(device, SamplerStateDescription);

            Material = new Materials.Silver();

            ConstantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MaterialBuf = new Buffer(device, Utilities.SizeOf<MaterialBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MatricesBuf = new Buffer(device, Utilities.SizeOf<Matrix>() * 4, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            
            MaterialBufStruct = new MaterialBufferStruct
            {
                Absorption = new Vector4(Material.Absorption, 0f),
                Ambient = new Vector4(Material.Ambient, 0f),
                Diffuse = new Vector4(Material.Diffuse, 0f),
                Shiness = Material.Shiness
            };
        }

        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];

            var quat = Quaternion.RotationMatrix(Rotation);

            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);
            WorldPosition = Vector3.Transform(InitialPosition, Transform);
            for (var i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, Transform);
                ret[i].Normal = Vector4.Transform(vertices[i].Normal, Transform);
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView)
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            InitialVertices = Vertices;
            Vertices = Transformation(Vertices, InitialPosition, Rotation);
            VertexBuffer = Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        {
            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            WorldPosition = Vector3.Transform(InitialPosition, Transform);
            var worldViewProj = Transform * view * proj; 

            deviceContext.PixelShader.SetShaderResource(0, TextureView);
            deviceContext.PixelShader.SetSampler(0, Sampler);
            ////Material Buffer
            //deviceContext.UpdateSubresource(ref MaterialBufStruct, MaterialBuf, 0);
            //deviceContext.PixelShader.SetConstantBuffer(0, MaterialBuf);
            var matrixStruct = new MatrixBufferStruct
            {
                World = Transform,
                View = view,
                Proj = proj
            };

            deviceContext.UpdateSubresource(ref matrixStruct, MatricesBuf);
            deviceContext.VertexShader.SetConstantBuffer(3, MatricesBuf);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(VerticesCount, 0);
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
            Sampler.Dispose();
            Texture.Dispose();
            TextureView.Dispose();
            MatricesBuf.Dispose();
            MaterialBuf.Dispose();
            ConstantBuffer.Dispose();
        }
    }
}
