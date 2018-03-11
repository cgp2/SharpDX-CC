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

        private const int VertexCount = 100;
        public float Diametr = 0.3f;
        public float Radius;

        public CircleComponent(Direct3D11.Device device)
        {
            this.Device = device;
            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            Radius = Diametr / 2;
            indices = new int[VertexCount];
            InitialPoints = new Vector4[VertexCount*2];

            var s = 0;
            for (var i = 0; i < VertexCount*2; i += 2)
            {
                var x = (float)(Radius * Math.Cos(i * (2 * Math.PI / VertexCount)));
                var y = (float)(Radius * Math.Sin(i * (2 * Math.PI / VertexCount)));
                InitialPoints[i] = new Vector4(x, y, 0f, 1.0f);
                InitialPoints[i + 1] = Color.Blue.ToVector4();
                indices[s] = s;
                s++;           
            }


            GlobalVertices = InitialPoints;
            VertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, GlobalVertices);

            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            ConstantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public Vector4[] Transformation(Vector4[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new Vector4[vertices.Length];
            var transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0f, 0f, 0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i += 2)
            {
                ret[i] = Vector4.Transform(vertices[i], transform);
                ret[i + 1] = vertices[i + 1];
            }

            return ret;
        }

        public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView)
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            GlobalVertices = Transformation(InitialPoints, InitialPosition, Rotation);
            VertexBuffer = Direct3D11.Buffer.Create(Device, Direct3D11.BindFlags.VertexBuffer, GlobalVertices);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        {
            var transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            var worldViewProj = transform * view * proj;

            deviceContext.VertexShader.SetConstantBuffer(0, ConstantBuffer);
            deviceContext.UpdateSubresource(ref worldViewProj, ConstantBuffer, 0);
           
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));
            deviceContext.Draw(InitialPoints.Count(), 0);

            Translation = new Vector3(0,0,0);
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();        
            ConstantBuffer.Dispose();
        }
    }
}
