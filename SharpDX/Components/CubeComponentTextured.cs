using System;
using System.Linq;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Materials;


namespace SharpDX.Components
{
    class CubeComponentTextured : AbstractComponent
    {
        Vector2[] textCoords;

        //public CubeComponentTextured(Direct3D11.Device device, string pathToTexture)
        //{
        //    this.Device = device;

           

        //    Texture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), pathToTexture));
        //    TextureView = new ShaderResourceView(device, Texture);
        //    SamplerStateDescription = new SamplerStateDescription
        //    {
        //        AddressU = TextureAddressMode.Wrap,
        //        AddressV = TextureAddressMode.Wrap,
        //        AddressW = TextureAddressMode.Wrap,
        //        Filter = Filter.MinMagMipLinear,
        //    };
        //    Sampler = new SamplerState(device, SamplerStateDescription);

        //    InitialPosition = new Vector3(0f, 0f, 0f);
        //    RotationCenter = InitialPosition;
        //    Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
        //    Translation = new Vector3(0f, 0f, 0f);
        //    ScalingCenter = InitialPosition;
        //    Scaling = new Vector3(1f, 1f, 1f);

        //    VertexBuffer = Direct3D11.Buffer.Create(device, Direct3D11.BindFlags.VertexBuffer, Vertices);
        //    ConstantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
        //        BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        //    MaterialBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<MaterialBufferStruct>(),
        //        ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None,
        //        ResourceOptionFlags.None, 0);
        //    MatricesBuf = new Direct3D11.Buffer(device, Utilities.SizeOf<MatrixBufferStruct>(),
        //        ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None,
        //        ResourceOptionFlags.None, 0);
        //    ShadowTransformBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(),
        //        ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None,
        //        ResourceOptionFlags.None, 0);
        //    ShadowWorldBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<MatrixBufferStruct>(),
        //        ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None,
        //        ResourceOptionFlags.None, 0);

        //    Material = new Silver();
        //    MaterialBufStruct = new MaterialBufferStruct
        //    {
        //        Absorption = new Vector4(Material.Absorption, 0f),
        //        Ambient = new Vector4(Material.Ambient, 0f),
        //        Diffuse = new Vector4(Material.Diffuse, 0f),
        //        Shiness = Material.Shiness
        //    };
        //}

        protected override void InitializeVertices(string filePath)
        {
            InitialPoints = new Vector4[]
           {
                    new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                    new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // BACK
                    new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                    new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                    new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                    new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                    new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                    new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
           };

            textCoords = new Vector2[]
            {
                new Vector2(0.25f, 0.66f), // Front
                new Vector2(0.25f, 0.33f),
                new Vector2(0.5f, 0.33f),
                new Vector2(0.25f, 0.66f),
                new Vector2(0.5f, 0.33f),
                new Vector2(0.5f, 0.66f),
                new Vector2(0.25f, 0.66f), // BACK
                new Vector2(0.75f, 0.66f),
                new Vector2(0.75f, 0.33f),
                new Vector2(0.75f, 0.66f),
                new Vector2(1f, 0.66f),
                new Vector2(1f, 0.33f),
                new Vector2(0.25f, 0.33f), // Top
                new Vector2(0.25f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.25f, 0.33f),
                new Vector2(0.5f, 0.33f),
                new Vector2(0.5f, 0f),
                new Vector2(0.25f, 1f),    // Bottom
                new Vector2(0.5f, 0.66f),
                new Vector2(0.25f, 0.66f),
                new Vector2(0.25f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.66f),
                new Vector2(0f, 0.66f),    // Left
                new Vector2(0f, 0.33f),
                new Vector2(0.25f, 0.33f),
                new Vector2(0f, 0.66f),
                new Vector2(0.25f, 0.33f),
                new Vector2(0.25f, 0.66f),
                new Vector2(0.5f, 0.66f),  // Right
                new Vector2(0.75f, 0.33f),
                new Vector2(0.5f, 0.33f),
                new Vector2(0.5f, 0.66f),
                new Vector2(0.75f, 0.66f),
                new Vector2(0.75f, 0.33f),
            };

            Vertices = new VertexPositionNormalTexture[textCoords.Count()];
            for (int i = 0; i < InitialPoints.Count(); i += 2)
                Vertices[i / 2] = new VertexPositionNormalTexture(InitialPoints[i], Color.Blue.ToColor4(), textCoords[i / 2]);

            InitialVertices = Vertices;
        }


        //public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        //{
        //    Matrix transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
        //    WorldPosition = Vector3.Transform(InitialPosition, transform);
        //    var shdTr = transform * ShadowTransform;

        //    deviceContext.PixelShader.SetShaderResource(0, TextureView);
        //    deviceContext.PixelShader.SetSampler(0, Sampler);
        //    //Material Buffer
        //    deviceContext.UpdateSubresource(ref MaterialBufStruct, MaterialBuf, 0);
        //    deviceContext.PixelShader.SetConstantBuffer(0, MaterialBuf);
        //    var matrixStruct = new MatrixBufferStruct
        //    {
        //        World = Transform,
        //        View = view,
        //        Proj = proj
        //    };

        //    deviceContext.UpdateSubresource(ref matrixStruct, MatricesBuf);
        //    deviceContext.VertexShader.SetConstantBuffer(3, MatricesBuf);

        //    //ShadowBuf
        //    deviceContext.UpdateSubresource(ref shdTr, ShadowTransformBuffer);
        //    deviceContext.VertexShader.SetConstantBuffer(4, ShadowTransformBuffer);

        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionColorTexture>(), 0));

        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        //    deviceContext.Draw(Vertices.Count(), 0);
        //}

        public CubeComponentTextured(Device device) : base(device)
        {
        }
    }
}
