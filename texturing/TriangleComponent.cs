using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using System.Diagnostics;
using Vector2 = SharpDX.Vector2;
using Direct3D11 = SharpDX.Direct3D11;
using Direct3D = SharpDX.Direct3D;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Vector4 = SharpDX.Vector4;
using Vector3 = SharpDX.Vector3;
using Color = SharpDX.Color;
using Matrix = SharpDX.Matrix;
using Quaternion = SharpDX.Quaternion;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace texturing
{
    class TriangleComponent : AbstractComponent
    {
        public TriangleComponent(Direct3D11.Device device)
        {
            this.device = device;

            vertices = new Vector4[]
        {
            new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color.Red.ToVector4(),
            new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color.Blue.ToVector4(),
            new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color.Green.ToVector4(),
        };
            WorldPosition = new Vector3(0f, 0f, 0f);
            Translation = Matrix.Translation(new Vector3(0f, 0f, 0f));
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Scaling = Matrix.Scaling(1);

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
            constantBuffer = new Direct3D11.Buffer(device, SharpDX.Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, Direct3D11.BindFlags.ConstantBuffer, Direct3D11.CpuAccessFlags.None, Direct3D11.ResourceOptionFlags.None, 0);
        }

        public Vector4[] Transformation(Vector4[] vertices, Vector3 translation, Matrix rotation)
        {
            Vector4[] ret = new Vector4[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i += 2)
            {
                ret[i] = Vector4.Transform(vertices[i], transform);
                ret[i + 1] = vertices[i + 1];
            }

            return ret;
        }

        public override void Update()
        {
            vertices = Transformation(vertices, WorldPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
        }


        public override void Draw(DeviceContext deviceContext, Matrix proj, Direct3D11.Buffer initialConstantBuffer)
        {
            var worldViewProj = Rotation * Translation * Scaling * proj;

            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            //float brightness = 1;
            //deviceContext.UpdateSubresource(ref brightness, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.PrimitiveTopology = Direct3D.PrimitiveTopology.TriangleList;

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, SharpDX.Utilities.SizeOf<Vector4>() * 2, 0));
            deviceContext.Draw(vertices.Count(), 0);

            deviceContext.VertexShader.SetConstantBuffer(0, initialConstantBuffer);
        }


    }
}
