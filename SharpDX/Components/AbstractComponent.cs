using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace SharpDX.Components
{
    public abstract class AbstractComponent
    {
        protected Device Device;
        protected Direct3D11.Buffer VertexBuffer;
        protected Direct3D11.Buffer ConstantBuffer;
        public Direct3D11.Buffer IndexBuffer;

        protected Vector4[] InitialPoints;
        public Vector4[] GlobalVertices;
        public Vector3 InitialPosition;
        public Vector3 Translation;
        public Matrix Rotation;
        public Vector3 Scaling;
        public Vector3 ScalingCenter;
        public Vector3 RotationCenter;
        public VertexPositionNormalTexture[] Vertices;
        public VertexPositionNormalTexture[] InitialVertices;
        public Matrix Transform;
        public Vector4 WorldPosition;

        protected Texture2D Texture;
        protected ShaderResourceView TextureView;
        protected SamplerStateDescription SamplerStateDescription;
        protected SamplerState Sampler;
        public int VerticesCount;

        protected Materials.AbstractMaterial Material;
        public Direct3D11.Buffer MaterialBuf;
        public Direct3D11.Buffer MatricesBuf;
        public MaterialBufferStruct MaterialBufStruct;

        protected Direct3D11.Buffer ShadowWorldBuffer;
        protected Direct3D11.Buffer ShadowTransformBuffer;

        public Matrix ShadowTransform;

        public abstract void Draw(Direct3D11.DeviceContext deviceContext, Matrix proj, Matrix view);
        public abstract void DrawShadow(Direct3D11.DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView);

        protected void InitializeResources()
        {
            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);

            ConstantBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MaterialBuf = new Direct3D11.Buffer(Device, Utilities.SizeOf<MaterialBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            MatricesBuf = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowTransformBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowWorldBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(),ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            MaterialBufStruct = new MaterialBufferStruct
            {
                Absorption = new Vector4(Material.Absorption, 0f),
                Ambient = new Vector4(Material.Ambient, 0f),
                Diffuse = new Vector4(Material.Diffuse, 0f),
                Shiness = Material.Shiness
            };

        }

        public abstract void Update();

        public abstract void Dispose();

        public struct MaterialBufferStruct
        {
            public Vector4 Diffuse;
            public Vector4 Absorption;
            public Vector4 Ambient;
            public float Shiness;
            public float Dum1, Dum2, Dum3;
        }

        public struct MatrixBufferStruct
        {
            public Matrix World;
            public Matrix View;
            public Matrix Proj;
        }
    }
}