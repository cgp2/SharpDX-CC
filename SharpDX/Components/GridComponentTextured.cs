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
        public int TerrainWidth = 20, TerrainHeight = 20, SquareWidth, Lenght;
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }

        public Direct3D11.Buffer indexBuffer;

        Vector2[] textCoords;
        VertexPositionNormalTexture[] t;
        Texture2D texture;
        ShaderResourceView textureView;
        SamplerStateDescription samplerStateDescription;
        SamplerState sampler;

        Materials.AbstractMaterial material;
        Direct3D11.Buffer matBuf;
        Direct3D11.Buffer worldBuf;
        Direct3D11.Buffer viewBuf;
        Direct3D11.Buffer projBuf;
        MaterialBufferStruct MaterialBufStruct;
        Matrix shadowTransform;
        Direct3D11.Buffer shadowTransformBuffer;
        Direct3D11.Buffer shadowWorldBuffer;

        public GridComponentTextured(Direct3D11.Device device, int squareWidth, string pathTexture)
        {
            this.device = device;
            SquareWidth = squareWidth;
            Lenght = squareWidth * TerrainHeight;

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            VertexCount = (TerrainWidth - 1) * (TerrainHeight - 1) * 6;

            IndexCount = VertexCount;

            vertices = new Vector4[VertexCount];
            textCoords = new Vector2[VertexCount];
            t = new VertexPositionNormalTexture[VertexCount];

            int[] indices = new int[IndexCount];

            int index = 0;
            int s = 0;
            float positionX = 0f;
            float positionZ = 0f;
            Vector4 normal;
            for (int j = 0; j < (TerrainHeight - 1); j+=1)
            {
                for (int i = 0; i < (TerrainWidth - 1); i+= 1)
                {
                    // Bottom left.
                    positionX = (float)i * SquareWidth;
                    positionZ = (float)j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper left.
                    positionX = (float)i * SquareWidth;
                    positionZ = (float)(j * SquareWidth + SquareWidth);
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    // Upper right.
                    positionX = (float)(i * SquareWidth + SquareWidth);
                    positionZ = (float)(j * SquareWidth + SquareWidth);
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0.5f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;

                    // Bottom right.
                    positionX = (float)(i * SquareWidth + SquareWidth);
                    positionZ = (float)j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0.5f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;
                    //Bottom left.
                    positionX = (float)i * SquareWidth;
                    positionZ = (float)j * SquareWidth;
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0f, 0.5f));
                    indices[s] = s;
                    index += 1;
                    s++;

                    // Upper right.
                    positionX = (float)(i * SquareWidth + SquareWidth);
                    positionZ = (float)(j * SquareWidth + SquareWidth);
                    normal = new Vector4(0, 1f, 0, 0.0f);
                    t[index] = new VertexPositionNormalTexture(new Vector4(positionX, 0.0f, positionZ, 1.0f), normal, new Vector2(0.5f, 0f));
                    indices[s] = s;
                    index += 1;
                    s++;
                }
            }

            initialVertices = t;

            texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), pathTexture));
            textureView = new ShaderResourceView(device, texture);
            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear
            };
            sampler = new SamplerState(device, samplerStateDescription);

            material = new Materials.Silver();

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            matBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            shadowTransformBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            shadowWorldBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            worldBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            viewBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            projBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            MaterialBufStruct = new MaterialBufferStruct();
            MaterialBufStruct.Absorption = new Vector4(material.Absorption, 0f);
            MaterialBufStruct.Ambient = new Vector4(material.Ambient, 0f);
            MaterialBufStruct.Diffuse = new Vector4(material.Diffuse, 0f);
            MaterialBufStruct.Shiness = material.Shiness;

            indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);
        }

        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];

            var quat = Quaternion.RotationMatrix(Rotation);

            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out transform);
            WorldPosition = Vector3.Transform(InitialPosition, transform);
            for (int i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, transform);
                ret[i].Normal = Vector4.Transform(vertices[i].Normal, transform);
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public override void Update()
        {
            initialVertices = t;
            t = Transformation(t, InitialPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, bool toStreamOutput)
        {
            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out transform);

            WorldPosition = Vector3.Transform(InitialPosition, transform);

            var shdTr = transform * shadowTransform;

            deviceContext.PixelShader.SetShaderResource(0, textureView);
            deviceContext.PixelShader.SetSampler(0, sampler);
            //Material Buffer
            deviceContext.UpdateSubresource(ref MaterialBufStruct, matBuf, 0);
            deviceContext.PixelShader.SetConstantBuffer(0, matBuf);
            //World buffer
            deviceContext.UpdateSubresource(ref transform, worldBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(4, worldBuf);
            //View Buffer
            deviceContext.UpdateSubresource(ref view, viewBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(5, viewBuf);
            //Proj Buffer
            deviceContext.UpdateSubresource(ref proj, projBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(6, projBuf);
            //ShadowBuf
            deviceContext.UpdateSubresource(ref shdTr, shadowTransformBuffer, 0);
            deviceContext.VertexShader.SetConstantBuffer(7, shadowTransformBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.Draw(VertexCount, 0);
        }

        public void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform)
        {
            this.shadowTransform = shadowTransform;

            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out transform);

            //World buffer
            deviceContext.UpdateSubresource(ref transform, shadowWorldBuffer, 0);
            deviceContext.VertexShader.SetConstantBuffer(2, shadowWorldBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(VertexCount, 0);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            constantBuffer.Dispose();
        }
    }
}
