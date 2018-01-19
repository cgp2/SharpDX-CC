using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.InputStructures
{
    public struct VertexPositionNormalWorldPosTexture
    {
        public Vector4 Position;
        public Vector4 Normal;
        public Vector4 WorldPosition;
        public Vector2 Texture;

        public VertexPositionNormalWorldPosTexture(Vector4 position, Vector4 normal, Vector4 worldPos, Vector2 textureCoord)
        {
            Position = position;
            Normal = normal;
            WorldPosition = worldPos;
            Texture = textureCoord;
        }
    }
}
