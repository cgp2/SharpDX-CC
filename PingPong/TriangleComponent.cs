using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;

namespace PingPong
{
    class TriangleComponent
    {
        public Vector3 worldPosition = new Vector3(2f, 0f, 2f);
        public Matrix rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
        private Vector4[] vertices = new Vector4[]
        {
            new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color.Red.ToVector4(),
            new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color.Blue.ToVector4(),
            new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color.Green.ToVector4(),
        };
        private Vector4[] globalVertices;

        public Direct3D11.Buffer vertexBuffer;


        public TriangleComponent(Direct3D11.Device device)
        {
            globalVertices = vertices;
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, globalVertices);
        }

        public Vector4[] Transformation(Vector4[] vertices, Vector3 translation, Matrix rotation)
        {
            Vector4[] ret = new Vector4[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);

            for(int i =0; i < vertices.Length; i+=2)
            {
                ret[i] = Vector4.Transform(vertices[i], transform);
                ret[i + 1] = vertices[i + 1];
            }

            return ret;
        }

        public void Update(Direct3D11.Device device)
        {
            globalVertices = Transformation(vertices, worldPosition, rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, globalVertices);
        }


        public void Draw(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>()*2, 0));
            deviceContext.Draw(vertices.Count(), 0);
        }

    
    }
}
