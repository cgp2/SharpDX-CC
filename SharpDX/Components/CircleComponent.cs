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

namespace SharpDX.Components
{
    class CircleComponent : AbstractComponent
    {

        private int[] indices;

        private Direct3D11.Buffer indexBuffer;

        private int vertexCount = 100;
        public float Diametr = 0.3f;
        public float Radius;

        public CircleComponent(Direct3D11.Device device)
        {
            this.device = device;
            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            Radius = Diametr / 2;
            indices = new int[vertexCount];
            vertices = new Vector4[vertexCount*2];

            int s = 0;
            for (int i = 0; i < vertexCount*2; i += 2)
            {
                float x = (float)(Radius * Math.Cos(i * (2 * Math.PI / vertexCount)));
                float y = (float)(Radius * Math.Sin(i * (2 * Math.PI / vertexCount)));
                vertices[i] = new Vector4(x, y, 0f, 1.0f);
                vertices[i + 1] = Color.Blue.ToVector4();
                indices[s] = s;
                s++;           
            }


            GlobalVertices = vertices;
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, GlobalVertices);

            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public Vector4[] Transformation(Vector4[] vertices, Vector3 translation, Matrix rotation)
        {
            Vector4[] ret = new Vector4[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0f, 0f, 0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i += 2)
            {
                ret[i] = Vector4.Transform(vertices[i], transform);
                ret[i + 1] = vertices[i + 1];
            }

            return ret;
        }

        public override void Update()
        {
            GlobalVertices = Transformation(vertices, InitialPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, GlobalVertices);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, Direct3D11.Buffer initialConstantBuffer)
        {
            Matrix transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            var worldViewProj = transform * view * proj;

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);
           
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));
            deviceContext.Draw(vertices.Count(), 0);

            deviceContext.VertexShader.SetConstantBuffer(0, initialConstantBuffer);

            Translation = new Vector3(0,0,0);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();        
            constantBuffer.Dispose();
        }
    }
}
