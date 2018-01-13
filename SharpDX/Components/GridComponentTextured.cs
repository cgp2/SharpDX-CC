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
    class GridComponentTextured : AbstractComponent
    {
        private int TerrainWidth = 100, TerrainHeight = 100;
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }

        public Direct3D11.Buffer indexBuffer;

        Vector2[] textCoords;
        VertexPositionColorTexture[] t;
        Texture2D texture;
        ShaderResourceView textureView;
        SamplerStateDescription samplerStateDescription;
        SamplerState sampler;

        public GridComponentTextured(Direct3D11.Device device)
        {
            this.device = device;

            WorldPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = WorldPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = WorldPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * 6;

            IndexCount = VertexCount;

            vertices = new Vector4[VertexCount];
            textCoords = new Vector2[VertexCount];
            t = new VertexPositionColorTexture[VertexCount];

            int[] indices = new int[IndexCount];

            int index = 0;
            int s = 0;
            float positionX = 0f;
            float positionZ = 0f; 
            for (int j = 0; j < (TerrainHeight - 1); j++)
            {
                for (int i = 0; i < (TerrainWidth - 1); i++)
                {
                    // Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0f, 0.5f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper left.
                    positionX = (float)i;
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0f, 0f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0.5f, 0f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;


                    // Bottom right.
                    positionX = (float)(i + 1);
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0.5f, 0.5f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;
                    //Bottom left.
                    positionX = (float)i;
                    positionZ = (float)j;
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0f, 0.5f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;

                    // Upper right.
                    positionX = (float)(i + 1);
                    positionZ = (float)(j + 1);
                    vertices[index] = new Vector4(positionX, 0.0f, positionZ, 1.0f);
                    textCoords[index] = new Vector2(0.5f, 0f);
                    t[index] = new VertexPositionColorTexture(vertices[index], Color.White.ToColor4(), textCoords[index]);
                    indices[s] = s;
                    index += 1;
                    s++;
                }
            }

            texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "1t.png"));
            textureView = new ShaderResourceView(device, texture);
            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear
            };
            sampler = new SamplerState(device, samplerStateDescription);

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);
        }

        public VertexPositionColorTexture[] Transformation(VertexPositionColorTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionColorTexture[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, transform);
                ret[i].Color = vertices[i].Color;
                ret[i].TextureCoordinates = vertices[i].TextureCoordinates;
            }

            return ret;
        }

        public void Update()
        {
            t = Transformation(t, WorldPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
        }


        public override void Draw(DeviceContext deviceContext, Matrix proj, Direct3D11.Buffer initialConstantBuffer)
        {
            var worldViewProj = proj;

            deviceContext.PixelShader.SetShaderResource(0, textureView);
            deviceContext.PixelShader.SetSampler(0, sampler);
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionColorTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.Draw(VertexCount, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, initialConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, initialConstantBuffer);
        }

      
    }
}
