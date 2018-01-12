using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Direct3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Device = SharpDX.Direct3D11.Device;


namespace texturing
{
    abstract class AbstractComponent
    {
        public Device device;
        protected Direct3D11.Buffer vertexBuffer;
        protected Direct3D11.Buffer constantBuffer;

        protected Vector4[] vertices ;
        public Vector4[] GlobalVertices;
        public Vector3 WorldPosition;
        public Matrix Translation;
        public Matrix Rotation;
        public Matrix Scaling;


        public abstract void Draw (Direct3D11.DeviceContext deviceContext, Matrix proj, Direct3D11.Buffer initialConstantBuffer);
        public abstract void Update();


    }
}
