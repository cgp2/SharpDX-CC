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
    class TriangleComponent : AbstractComponent
    {
        Vector2[] textCoords;
        Texture2D texture;
        ShaderResourceView textureView;
        SamplerStateDescription samplerStateDescription;
        SamplerState sampler;

        public TriangleComponent(Direct3D11.Device device)
        {
            this.device = device;

            vertices = new Vector4[]
            {
                new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color.Red.ToVector4(),
                new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color.Blue.ToVector4(),
                new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color.Green.ToVector4(),
            };

            textCoords = new Vector2[]
            {
                new Vector2(0.5f, 0.1f),
                new Vector2(1.0f, 0.8f),
                new Vector2(0.0f, 0.8f),
            };

            t = new VertexPositionNormalTexture[3];
            t[0] = new VertexPositionNormalTexture(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color.Red.ToVector4(), textCoords[0]);
            t[1] = new VertexPositionNormalTexture(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color.Red.ToVector4(), textCoords[1]);
            t[2] = new VertexPositionNormalTexture(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color.Red.ToVector4(), textCoords[2]);

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "3t.png"));
            textureView = new ShaderResourceView(device, texture);
            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagPointMipLinear
            };
            sampler = new SamplerState(device, samplerStateDescription);

            initialVertices = t;

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
            constantBuffer = new Direct3D11.Buffer(device, SharpDX.Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, Direct3D11.BindFlags.ConstantBuffer, Direct3D11.CpuAccessFlags.None, Direct3D11.ResourceOptionFlags.None, 0);
        }

        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];
            Matrix transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);

            for (int i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, transform);
                ret[i].Normal = vertices[i].Normal;
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public override void Update()
        {
            t = Transformation(t, InitialPosition, Rotation);
            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view, Direct3D11.Buffer initialConstantBuffer)
        {
            transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            WorldPosition = Vector3.Transform(InitialPosition, transform);
            var worldViewProj = transform * view * proj;

            deviceContext.PixelShader.SetShaderResource(0, textureView);
            deviceContext.PixelShader.SetSampler(0, sampler);
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            //float brightness = 1;
            //deviceContext.UpdateSubresource(ref brightness, constantBuffer, 0);

            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.InputAssembler.PrimitiveTopology = Direct3D.PrimitiveTopology.TriangleList;

            deviceContext.InputAssembler.SetVertexBuffers(0, new Direct3D11.VertexBufferBinding(vertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColorTexture>(), 0));
            deviceContext.Draw(vertices.Count(), 0);

            deviceContext.VertexShader.SetConstantBuffer(0, initialConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(0, initialConstantBuffer);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            sampler.Dispose();
            texture.Dispose();
            textureView.Dispose();
            constantBuffer.Dispose();
        }
    }

}
