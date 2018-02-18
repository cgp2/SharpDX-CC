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
using System.IO;
namespace SharpDX.Components
{
    class PlaneComponent : AbstractComponent
    {
        Texture2D texture;
        ShaderResourceView textureView;
        SamplerStateDescription samplerStateDescription;
        SamplerState sampler;
        int verticesCount;
        Materials.AbstractMaterial material;
        Direct3D11.Buffer matBuf;
        Direct3D11.Buffer worldBuf;
        Direct3D11.Buffer viewBuf;
        Direct3D11.Buffer projBuf;
        MaterialBufferStruct MaterialBufStruct;

        public PlaneComponent(Direct3D11.Device device, ShaderResourceView text)
        {
            this.device = device;

            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            vertices = new Vector4[]
            {
                new Vector4(-4.0f, 0.0f, -4.0f,  1.0f), 
                new Vector4(-4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4(-4.0f, 0.0f, -4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f,  4.0f,  1.0f),
                new Vector4( 4.0f, 0.0f, -4.0f,  1.0f),
            };

            var textCoords = new Vector2[]
            {
                new Vector2(0f, 1f), 
                new Vector2(0f, 0f),
                new Vector2(1, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),

            };

            var normals = new Vector4[]
            {
                new Vector4(-4.0f, 1.0f, -4.0f, 0.0f),
                new Vector4(-4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4(-4.0f, 1.0f, -4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f,  4.0f, 0.0f),
                new Vector4( 4.0f, 1.0f, -4.0f, 0.0f),
            };

            verticesCount = vertices.Count();
            t = new VertexPositionNormalTexture[verticesCount];
            for(int i = 0; i < verticesCount; i++)
            {
                t[i].Position = vertices[i];
                t[i].Normal = normals[i];
                t[i].Texture = textCoords[i];
            }

            initialVertices = t;

            vertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, t);

            textureView = text;

            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear,
            };
            sampler = new SamplerState(device, samplerStateDescription);;

            material = new Materials.Silver();

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            matBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<MaterialBufferStruct>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            worldBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            projBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            viewBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            MaterialBufStruct = new MaterialBufferStruct();
            MaterialBufStruct.Absorption = new Vector4(material.Absorption, 0f);
            MaterialBufStruct.Ambient = new Vector4(material.Ambient, 0f);
            MaterialBufStruct.Diffuse = new Vector4(material.Diffuse, 0f);
            MaterialBufStruct.Shiness = material.Shiness;
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
            var worldViewProj = transform * view * proj; ;

            deviceContext.PixelShader.SetShaderResource(0, textureView);
            deviceContext.PixelShader.SetSampler(0, sampler);
            ////Material Buffer
            //deviceContext.UpdateSubresource(ref MaterialBufStruct, matBuf, 0);
            //deviceContext.PixelShader.SetConstantBuffer(0, matBuf);
            //World buffer
            deviceContext.UpdateSubresource(ref transform, worldBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(4, worldBuf);
            //View Buffer
            deviceContext.UpdateSubresource(ref view, viewBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(5, viewBuf);
            //Proj Buffer
            deviceContext.UpdateSubresource(ref proj, projBuf, 0);
            deviceContext.VertexShader.SetConstantBuffer(6, projBuf);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(verticesCount, 0);
        }

        public override void Dispose()
        {
            vertexBuffer.Dispose();
            sampler.Dispose();
            texture.Dispose();
            textureView.Dispose();
            worldBuf.Dispose();
            matBuf.Dispose();
            viewBuf.Dispose();
            projBuf.Dispose();
            constantBuffer.Dispose();
        }
    }
}
