using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Materials;
using SharpDX.WIC;

namespace SharpDX.Components
{
    internal class GridComponentTextured : AbstractComponent
    {
        public const int TerrainWidth = 20;
        public const int TerrainHeight = 20;
        public int SquareWidth;
        public int Lenght;

        private int VertexCount { get; }
        public int IndexCount { get; }

        public GridComponentTextured(Device device, int squareWidth, string pathTexture)
        {
            Device = device;
            SquareWidth = squareWidth;
            Lenght = squareWidth * TerrainHeight;

            VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * 6;

            IndexCount = VertexCount;

            InitialPoints = new Vector4[VertexCount];
            Vertices = new VertexPositionNormalTexture[VertexCount];

            var indices = new int[IndexCount];

            var index = 0;
            var s = 0;            
            for (var j = 0; j < TerrainHeight - 1; j += 1)
            {
                for (var i = 0; i < TerrainWidth - 1; i += 1)
                {
                    // Bottom left.
                    var positionX = (float) i * SquareWidth;
                    var positionZ = (float) j * SquareWidth;
                    var normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper left.
                    positionX = (float) i * SquareWidth;
                    positionZ = j * SquareWidth + SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper right.
                    positionX = i * SquareWidth + SquareWidth;
                    positionZ = j * SquareWidth + SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0.5f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;

                    // Bottom right.
                    positionX = i * SquareWidth + SquareWidth;
                    positionZ = (float) j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0.5f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    //Bottom left.
                    positionX = (float) i * SquareWidth;
                    positionZ = (float) j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;

                    // Upper right.
                    positionX = i * SquareWidth + SquareWidth;
                    positionZ = j * SquareWidth + SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0.5f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;
                }
            }

            InitialVertices = Vertices;

            Texture = TextureLoader.CreateTexture2DFromBitmap(device,
                TextureLoader.LoadBitmap(new ImagingFactory2(), pathTexture));
            TextureView = new ShaderResourceView(device, Texture);
        
            Material = new Silver();

            MaterialBufStruct = new MaterialBufferStruct
            {
                Absorption = new Vector4(Material.Absorption, 0f),
                Ambient = new Vector4(Material.Ambient, 0f),
                Diffuse = new Vector4(Material.Diffuse, 0f),
                Shiness = Material.Shiness
            };

            InitializeResources();
            IndexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.IndexBuffer, indices);
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

            var shdTr = Transform * ShadowTransform;

            deviceContext.PixelShader.SetShaderResource(0, TextureView);
            deviceContext.PixelShader.SetSampler(0, Sampler);
            //Material Buffer
            deviceContext.UpdateSubresource(ref MaterialBufStruct, MaterialBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(0, MaterialBuf);

            //Matrix Buffer
            var matrixStruct = new MatrixBufferStruct
            {
                World = Transform,
                View = view,
                Proj = proj
            };
            deviceContext.UpdateSubresource(ref matrixStruct, MatricesBuf);
            deviceContext.VertexShader.SetConstantBuffer(3, MatricesBuf);

            //ShadowBuf
            deviceContext.UpdateSubresource(ref shdTr, ShadowTransformBuffer);
            deviceContext.VertexShader.SetConstantBuffer(4, ShadowTransformBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.Draw(VertexCount, 0);
        }

        public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj,
            Matrix lightView)
        {
            ShadowTransform = shadowTransform;

            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            //Matrix bufffer
            var matrixStruct = new MatrixBufferStruct
            {
                World = Transform,
                View = lightView,
                Proj = lightProj
            };
            deviceContext.UpdateSubresource(ref matrixStruct, ShadowWorldBuffer);
            deviceContext.VertexShader.SetConstantBuffer(0, ShadowWorldBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(VertexCount, 0);
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
            ConstantBuffer.Dispose();
        }
    }
}