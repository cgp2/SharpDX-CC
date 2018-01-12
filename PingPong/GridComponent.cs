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


namespace PingPong
{
    class GridComponent
    {

        private Vector4[] globalVertices;
        public Vector3 worldPosition = new Vector3(2f, 0f, 2f);
        public Matrix rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);

        private int TerrainWidth = 100, TerrainHeight = 100;
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }

        public Vector4[] vertices;


        protected Direct3D11.Buffer constantBuffer;
        public Direct3D11.Buffer indexBuffer;
        public Direct3D11.Buffer vertexBuffer;


        public GridComponent(Direct3D11.Device device)
        {
            VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * 8;

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

            globalVertices = vertices;

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, globalVertices);


            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);


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

        public void Update(Direct3D11.Device device)
        {
            globalVertices = Transformation(vertices, worldPosition, rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, globalVertices);
        }


        public void Draw(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vector4>()*2, 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            deviceContext.Draw(VertexCount, 0);
        }

        //protected void InitializeShaders()
        //{
        //    using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_5_0", ShaderFlags.Debug))
        //    {
        //        inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
        //        vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
        //    }

        //    using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_5_0", ShaderFlags.Debug))
        //    {
        //        pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
        //    }

        //    deviceContext.VertexShader.Set(vertexShader);
        //    deviceContext.PixelShader.Set(pixelShader);

        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

        //    inputLayout = new InputLayout(device, inputSignature, inputElements);
        //    deviceContext.InputAssembler.InputLayout = inputLayout;


        //    constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

        //    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

        //    renderTargetView = new RenderTargetView(device, backBuffer);
        //    depthBuffer = new Texture2D(device, new Texture2DDescription()
        //    {
        //        Format = Format.D32_Float_S8X24_UInt,
        //        ArraySize = 1,
        //        MipLevels = 1,
        //        Width = renderForm.ClientSize.Width,
        //        Height = renderForm.ClientSize.Height,
        //        SampleDescription = new SampleDescription(1, 0),
        //        Usage = ResourceUsage.Default,
        //        BindFlags = BindFlags.DepthStencil,
        //        CpuAccessFlags = CpuAccessFlags.None,
        //        OptionFlags = ResourceOptionFlags.None
        //    });

        //    depthView = new DepthStencilView(device, depthBuffer);

        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
        //}

        //public void Run()
        //{
        //    deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
        //    deviceContext.OutputMerger.SetTargets(depthView, renderTargetView);
        //    RenderLoop.Run(renderForm, RenderCallback);
        //}

        //private void RenderCallback()
        //{
        //    Draw();
        //}

        //public void Draw()
        //{
        //    deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
        //    deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

        //    deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);
        //    deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
        //    deviceContext.OutputMerger.SetRenderTargets(renderTargetView);
        //    //deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

        //    //deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
        //    deviceContext.Draw(VertexCount, 0);

        //    swapChain.Present(1, PresentFlags.None);
        //}
    }
}
