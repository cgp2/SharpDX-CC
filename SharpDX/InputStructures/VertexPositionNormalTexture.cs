using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX
{
    public struct VertexPositionNormalTexture
    {
        public Vector4 Position;
        public Vector4 Normal;
        public Vector2 Texture;

        public VertexPositionNormalTexture(Vector4 position, Vector4 normal, Vector2 textureCoord)
        {
            Position = position;
            Normal = normal;
            Texture = textureCoord;
        }
    }
}
