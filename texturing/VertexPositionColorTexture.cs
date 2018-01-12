using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = SharpDX.Vector2;
using Direct3D11 = SharpDX.Direct3D11;
using Direct3D = SharpDX.Direct3D;
using Vector3 = SharpDX.Vector3;
using Color4 = SharpDX.Color4;

namespace texturing
{
   public struct VertexPositionColorTexture
    {
        public Vector3 Position;
        public Color4 Color;
        public Vector2 TextureCoordinates;

        public VertexPositionColorTexture(Vector3 position, Color4 color, Vector2 textureCoord)
        {
            Position = position;
            Color = color;
            TextureCoordinates = textureCoord;
        }
    }
}
