using SharpDX;
using System.Runtime.InteropServices;

namespace SharpDX
{
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
}