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
    class BoxComponent : AbstractComponent
    {
        public BoxComponent(Direct3D11.Device device)
        {
            this.device = device;

            vertices = new Vector4[]
            {
                    //TOP
                    new Vector4(-1.0f, 4.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    new Vector4(1.0f, 4.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    //BOTTOM
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    new Vector4(-1.0f, 0.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    //LEFT
                    new Vector4(-1.0f, 0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    new Vector4(-1.0f, 4.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    //RIGHT
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
                    new Vector4(1.0f, 4.0f, 0.0f, 1.0f), Color4.White.ToVector4(),
             };

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            GlobalVertices = vertices;

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, GlobalVertices);
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


        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, bool toStreamOutput)
        {
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            deviceContext.Draw(GlobalVertices.Count(), 0);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            constantBuffer.Dispose();
        }
    }
}
