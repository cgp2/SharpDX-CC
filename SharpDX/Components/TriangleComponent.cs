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
        VertexPositionColorTexture[] t;
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

            t = new VertexPositionColorTexture[3];
            t[0] = new VertexPositionColorTexture(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color.Red, textCoords[0]);
            t[1] = new VertexPositionColorTexture(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color.Red, textCoords[1]);
            t[2] = new VertexPositionColorTexture(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color.Red, textCoords[2]);

            WorldPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = WorldPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = WorldPosition;
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

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);
            constantBuffer = new Direct3D11.Buffer(device, SharpDX.Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, Direct3D11.BindFlags.ConstantBuffer, Direct3D11.CpuAccessFlags.None, Direct3D11.ResourceOptionFlags.None, 0);
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
            Matrix transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            var worldViewProj = transform * proj;

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
    }

}
