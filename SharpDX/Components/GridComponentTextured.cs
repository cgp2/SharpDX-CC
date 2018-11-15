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

        private int vertexCount;
        public int IndexCount;

        protected override void InitializeVertices(string filePath)
        {
            SquareWidth = 20;
            Lenght = 20 * TerrainHeight;

            vertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * 6;

            IndexCount = vertexCount;

            InitialPoints = new Vector4[vertexCount];
            Vertices = new VertexPositionNormalTexture[vertexCount];

            var indices = new int[IndexCount];

            var index = 0;
            var s = 0;
            for (var j = 0; j < TerrainHeight - 1; j += 1)
            {
                for (var i = 0; i < TerrainWidth - 1; i += 1)
                {
                    // Bottom left.
                    var positionX = (float)i * SquareWidth;
                    var positionZ = (float)j * SquareWidth;
                    var normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper left.
                    positionX = (float)i * SquareWidth;
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
                    positionZ = (float)j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    Vertices[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f),
                        normal,
                        new Vector2(0.5f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    //Bottom left.
                    positionX = (float)i * SquareWidth;
                    positionZ = (float)j * SquareWidth;
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
            VerticesCount = vertexCount;
            // IndexBuffer = Buffer.Create(Device, BindFlags.IndexBuffer, indices);
        }

        //public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj,
        //    Matrix lightView)
        //{
        //    ShadowTransform = shadowTransform;

        //    var quat = Quaternion.RotationMatrix(Rotation);
        //    Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

        //    //Matrix bufffer
        //    var matrixStruct = new MatrixBufferStruct
        //    {
        //        World = Transform,
        //        View = lightView,
        //        Proj = lightProj
        //    };
        //    deviceContext.UpdateSubresource(ref matrixStruct, ShadowWorldBuffer);
        //    deviceContext.VertexShader.SetConstantBuffer(0, ShadowWorldBuffer);

        //    deviceContext.InputAssembler.SetVertexBuffers(0,
        //        new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        //    deviceContext.Draw(vertexCount, 0);
        //}

        //public override void Dispose()
        //{
        //    VertexBuffer.Dispose();
        //    ConstantBuffer.Dispose();
        //}
        public GridComponentTextured(Device device) : base(device)
        {
        }
    }
}