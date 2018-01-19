using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;
using System.IO;


namespace SharpDX.Components
{
    class GridComponent : AbstractComponent
    {
        private int TerrainWidth = 100, TerrainHeight = 100;
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }

        public Direct3D11.Buffer indexBuffer;


        public GridComponent(Direct3D11.Device device)
        {
            this.device = device;

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            VertexCount = (TerrainWidth) * (TerrainHeight) * 8;

            IndexCount = VertexCount;

            vertices = new Vector4[VertexCount*2];

            int[] indices = new int[IndexCount];

            int index = 0;
            int s = 0;
            for (int j = 0; j < (TerrainHeight - 1); j++)
            {
                for (int i = 0; i < (TerrainWidth - 1); i++)
                {
                    // LINE 1
                    // Upper left.
                    float positionX = (float)i;
                    float positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index+=2;
                    s++;
                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ,1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 2
                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Bottom right.
                    positionX = (float)(i + 1);
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 3
                    // Bottom right.
                    positionX = (float)(i + 1);
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1]= new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 4
                    // Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ,1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Upper left.
                    positionX = (float)i;
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    vertices[index+1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                }
            }

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

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
            vertices = Transformation(vertices, InitialPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, vertices);
        }


        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, Direct3D11.Buffer initialConstantBuffer)
        {
            var worldViewProj = view * proj;

            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>()*2, 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            deviceContext.Draw(VertexCount, 0);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            constantBuffer.Dispose();
        }
    }
}
