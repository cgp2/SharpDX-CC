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

        private int[] indices;
        public Vector4 Position;
        public float Radius;    

        protected override void InitializeVertices(string filePath)
        {
            float diametr = 1.0f;
            int tessellation = 16;
            var verticalSegments = tessellation;
            var horizontalSegments = tessellation * 2;

            Vertices = new VertexPositionNormalTexture[(verticalSegments + 1) * (horizontalSegments + 1)];
            indices = new int[verticalSegments * (horizontalSegments + 1) * 6];

            Diametr = diametr;
            Radius = diametr / 2;

            var vertCount = 0;
            for (var i = 0; i <= verticalSegments; i++)
            {
                var lat = (float)(i * Math.PI / verticalSegments - Math.PI / 2);

                var cosX = (float)Math.Cos(lat);
                var y = (float)Math.Sin(lat);
                var cosZ = (float)Math.Cos(lat);

                var v = 1.0f - (float)i / verticalSegments;

                for (var j = 0; j <= horizontalSegments; j++)
                {
                    var u = (float)j / horizontalSegments;

                    var lon = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var x = cosX * (float)Math.Sin(lon);
                    var z = cosZ * (float)Math.Cos(lon);

                    Vertices[vertCount].Position = new Vector4(x, y, z, 1.0f) * Radius;
                    Vertices[vertCount].Normal = new Vector4(x, y, z, 0);
                    Vertices[vertCount].Texture = new Vector2(u, v);
                    vertCount++;

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

        public SphereComponent(Device device) : base(device)
        {
        }
    }
}