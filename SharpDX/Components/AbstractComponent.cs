using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace SharpDX.Components
{
    public abstract class AbstractComponent
    {
        public Device device;
        protected Direct3D11.Buffer vertexBuffer;
        protected Direct3D11.Buffer constantBuffer;

        protected Vector4[] vertices ;
        public Vector4[] GlobalVertices;
        public Vector3 InitialPosition;
        public Vector3 Translation;
        public Matrix Rotation;
        public Vector3 Scaling;
        public Vector3 ScalingCenter;
        public Vector3 RotationCenter;
        public VertexPositionNormalTexture[] t;
        public VertexPositionNormalTexture[] initialVertices;
        public Matrix transform;
        public Vector4 WorldPosition;

        public abstract void Draw (Direct3D11.DeviceContext deviceContext, Matrix proj, Matrix view, Direct3D11.Buffer initialConstantBuffer);

        public abstract void Update();

        public abstract void Dispose();

        public struct MaterialBufferStruct
        {
            public Vector4 Diffuse;
            public Vector4 Absorption;
            public Vector4 Ambient;
            public float Shiness;
        }

        public struct MatrixBufferStruct
        {
            public Matrix World;
            public Matrix worldViewProj;
            public Matrix View;
            public Matrix Proj;
        }
    }
}
