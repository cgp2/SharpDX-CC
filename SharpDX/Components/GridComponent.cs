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
using Device = SharpDX.Direct3D11.Device;


namespace SharpDX.Components
{
    class GridComponent : AbstractComponent
    {
        private int terrainWidth = 100, terrainHeight = 100;
        private int VertexCount;
        public int IndexCount;


        protected override void InitializeVertices(string filePath)
        {
            VertexCount = (terrainWidth) * (terrainHeight) * 8;

            IndexCount = VertexCount;

            InitialPoints = new Vector4[VertexCount * 2];

            int[] indices = new int[IndexCount];

            int index = 0;
            int s = 0;
            for (int j = 0; j < (terrainHeight - 1); j++)
            {
                for (int i = 0; i < (terrainWidth - 1); i++)
                {
                    // LINE 1
                    // Upper left.
                    float positionX = (float)i;
                    float positionZ = (float)(j + 1);
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 2
                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Bottom right.
                    positionX = (float)(i + 1);
                    positionZ = (float)j;
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 3
                    // Bottom right.
                    positionX = (float)(i + 1);
                    positionZ = (float)j;
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // LINE 4
                    // Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                    // Upper left.
                    positionX = (float)i;
                    positionZ = (float)(j + 1);
                    InitialPoints[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    InitialPoints[index + 1] = new Color4(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    indices[s] = s;
                    index += 2;
                    s++;
                }
            }

            VerticesCount = VertexCount;
        }

        //public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        //{
        //    var worldViewProj = view * proj;

        //    deviceContext.UpdateSubresource(ref worldViewProj, ConstantBuffer, 0);

        //    deviceContext.VertexShader.SetConstantBuffer(0, ConstantBuffer);

        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vector4>()*2, 0));

        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

        //    deviceContext.Draw(VertexCount, 0);
        //}

        //public override void Dispose()
        //{
        //    VertexBuffer.Dispose();
        //    ConstantBuffer.Dispose();
        //}
        public GridComponent(Device device) : base(device)
        {
        }
    }
}
