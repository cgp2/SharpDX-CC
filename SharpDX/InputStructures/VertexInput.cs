using SharpDX;
using System.Runtime.InteropServices;

namespace SharpDX
{
    public enum InputElements {VertexPosCol, VertexPosColTex, VertexPosNormTex, VertexPosNormWorldPos, TrgPosNormTex, TrgAdjPosNormTex, TrgStripPosNormTex, ShadowMapInput };

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor
    {
        public Vector4 Position;
        public Color4 Color;

        public VertexPositionColor(Vector4 position, Color4 color)
        {
            Position = position;
            Color = color;
        }
    }

    public struct VertexPositionColorTexture
    {
        public Vector4 Position;
        public Color4 Color;
        public Vector2 TextureCoordinates;

        public VertexPositionColorTexture(Vector4 position, Color4 color, Vector2 textureCoord)
        {
            Position = position;
            Color = color;
            TextureCoordinates = textureCoord;
        }
    }

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