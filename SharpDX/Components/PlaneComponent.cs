using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace SharpDX.Components
{
    internal class PlaneComponent : AbstractComponent
    {

        protected override void InitializeVertices(string filePath)
        {
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
            for (var i = 0; i < VerticesCount; i++)
            {
                Vertices[i].Position = InitialPoints[i];
                Vertices[i].Normal = normals[i];
                Vertices[i].Texture = textCoords[i];
            }

            InitialVertices = Vertices;
        }

        //public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        //{
        //    var ret = new VertexPositionNormalTexture[vertices.Length];

        //    var quat = Quaternion.RotationMatrix(Rotation);

        //    Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);
        //    WorldPosition = Vector3.Transform(InitialPosition, Transform);
        //    for (var i = 0; i < vertices.Length; i++)
        //    {
        //        ret[i].Position = Vector4.Transform(vertices[i].Position, Transform);
        //        ret[i].Normal = Vector4.Transform(vertices[i].Normal, Transform);
        //        ret[i].Texture = vertices[i].Texture;
        //    }

        //    return ret;
        //}

        public PlaneComponent(Device device) : base(device)
        {

        }
    }
}
