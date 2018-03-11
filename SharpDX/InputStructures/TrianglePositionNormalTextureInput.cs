using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.InputStructures
{
    public struct TrianglePositionNormalTextureInput
    {
        public VertexPositionNormalTexture Point1;
        public VertexPositionNormalTexture Point2;
        public VertexPositionNormalTexture Point3;
    }

    public struct TrianglePositionNormalTextureAdjInput
    {
        public VertexPositionNormalTexture Point1;
        public VertexPositionNormalTexture Point2;
        public VertexPositionNormalTexture Point3;
        public VertexPositionNormalTexture Point4;
        public VertexPositionNormalTexture Point5;
        public VertexPositionNormalTexture Point6;
    }

    public struct TrianglePositionNormalTextureStripInput
    {
        public VertexPositionNormalTexture[] Points;
    }

    public struct Triangle
    {
        public Vector4 Point1;
        public Vector4 Point2;
        public Vector4 Point3;
    }
}
