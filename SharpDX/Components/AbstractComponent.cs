using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.WIC;
using SharpDX.InputStructures;
using Bitmap = SharpDX.WIC.Bitmap;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;


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
        public Matrix Transform;
        public Vector4 WorldPosition;
        public int VerticesCount;

        public VertexPositionNormalTexture[] Vertices;
        public VertexPositionNormalTexture[] InitialVertices;

        protected Texture2D Texture;
        protected Texture2D[] TexturesCube;
        protected ShaderResourceView TextureView;
        protected SamplerStateDescription SamplerStateDescription;
        protected SamplerState Sampler;

        protected Materials.AbstractMaterial Material;
        public Direct3D11.Buffer MaterialBuf;
        public Direct3D11.Buffer MatricesBuf;
        public Direct3D11.Buffer FlagsBuf;
        public MaterialBufferStruct MaterialBufStruct;

        protected Direct3D11.Buffer ShadowWorldBuffer;
        protected Direct3D11.Buffer ShadowTransformBuffer;

        public Matrix ShadowTransform;

        public ShaderManager MainShaderManager;
        public ShaderManager ShadowShaderManager;

        public bool isTextureCube = false;
        public bool isLighten = true;

        protected AbstractComponent(Device device)
        {
            Device = device;
            Initialize(device);
        }

        protected void Initialize(Device device)
        {
            Device = device;
            InitialPosition = new Vector3(0f, 0f, 0f);
            RotationCenter = InitialPosition;
            Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
            Translation = new Vector3(0f, 0f, 0f);
            ScalingCenter = InitialPosition;
            Scaling = new Vector3(1f, 1f, 1f);

            MainShaderManager = new ShaderManager();
            ShadowShaderManager = new ShaderManager();
        }

        protected abstract void InitializeVertices(string filePath);

        public void InitializeResources(string texturePath, Materials.AbstractMaterial material, string pathVertShader, string pathPixShader, InputElements input, string vertFilePath = "")
        {
            InitializeVertices(vertFilePath);
            // Update();
            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);

            MainShaderManager.Initialize(Device, input, VertexBuffer, pathVertShader, pathPixShader);
            ShadowShaderManager.Initialize(Device, VertexBuffer, true);

            Material = material;

            Texture = TextureLoader.CreateTexture2DFromBitmap(Device, TextureLoader.LoadBitmap(new ImagingFactory2(), texturePath));
            TextureView = new ShaderResourceView(Device, Texture);

            ShadowTransformBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowWorldBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public void InitializeResources(string[] cubetexturePath, string pathVertShader, string pathPixShader, InputElements input, string vertFilePath = "")
        {
            InitializeVertices(vertFilePath);
            // Update();
            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);

            MainShaderManager.Initialize(Device, input, VertexBuffer, pathVertShader, pathPixShader);
            ShadowShaderManager.Initialize(Device, VertexBuffer, true);

            Material = new Materials.Silver();

            

            List<Texture2D> temttext = new List<Texture2D>();
            foreach(string texturePath in cubetexturePath)
            {
                temttext.Add(TextureLoader.CreateTexture2DFromBitmap(Device, TextureLoader.LoadBitmap(new ImagingFactory2(), texturePath)));
            }

            Texture2D texture2D = new Texture2D(Device, new Texture2DDescription
            {
                ArraySize = 6,
                MipLevels = 1,
                Height = temttext[0].Description.Height,
                Width = temttext[0].Description.Width,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = DXGI.Format.R8G8B8A8_UNorm,
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default //!!
            });

            TexturesCube = temttext.ToArray();

            isTextureCube = true;

            for (int i = 0; i < 6; i++)
            {
                Device.ImmediateContext.CopySubresourceRegion(TexturesCube[i], 0, null, texture2D, i);
                TexturesCube[i].Dispose();
            }

            TextureView = new ShaderResourceView(Device, texture2D);

            ShadowTransformBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowWorldBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public void InitializeResources(Bitmap texture, Materials.AbstractMaterial material, string pathVertShader, string pathPixShader, InputElements input, string vertFilePath = "")
        {
            InitializeVertices(vertFilePath);
            // this.Update();
            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);

            MainShaderManager.Initialize(Device, input, VertexBuffer, pathVertShader, pathPixShader);
            ShadowShaderManager.Initialize(Device, VertexBuffer, true);

            Material = material;

            Texture = TextureLoader.CreateTexture2DFromBitmap(Device, texture);
            TextureView = new ShaderResourceView(Device, Texture);

            ShadowTransformBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowWorldBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public void InitializeResources(ShaderResourceView textureResourceView, Materials.AbstractMaterial material, string pathVertShader, string pathPixShader, InputElements input, string vertFilePath = "")
        {
            InitializeVertices(vertFilePath);
            // this.Update();
            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);

            MainShaderManager.Initialize(Device, input, VertexBuffer, pathVertShader, pathPixShader);
            ShadowShaderManager.Initialize(Device, VertexBuffer, true);

            Material = material;

            TextureView = textureResourceView;

            ShadowTransformBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ShadowWorldBuffer = new Direct3D11.Buffer(Device, Utilities.SizeOf<MatrixBufferStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        //WTF IS THIS
        public VertexPositionNormalTexture[] Transformation(VertexPositionNormalTexture[] vertices, Vector3 translation, Matrix rotation)
        {
            var ret = new VertexPositionNormalTexture[vertices.Length];
            //Transform = Matrix.Transformation(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Quaternion.RotationMatrix(rotation), translation);
            var quat = Quaternion.RotationMatrix(Rotation);
            //Transform = Matrix.Transformation(ScalingCenter, Quaternion.Identity, Scaling, RotationCenter, Quaternion.RotationMatrix(Rotation), Translation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);
            WorldPosition = Vector3.Transform(InitialPosition, Transform);
            for (var i = 0; i < vertices.Length; i++)
            {
                ret[i].Position = Vector4.Transform(vertices[i].Position, Transform);
                ret[i].Normal = Vector4.Transform(vertices[i].Normal, Transform);
                ret[i].Texture = vertices[i].Texture;
            }

            return ret;
        }

        public void Update()
        {
            Vertices = Transformation(Vertices, InitialPosition, Rotation);
            VertexBuffer = Direct3D11.Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);
        }

        public void UpdateResources(Bitmap texture = null, Materials.AbstractMaterial material = null, string vertFilePath = "")
        {
            if (texture != null)
            {
                Texture?.Dispose();
                TextureView?.Dispose();
                Texture = TextureLoader.CreateTexture2DFromBitmap(Device, texture);
                TextureView = new ShaderResourceView(Device, Texture);
            }

            if (material != null)
            {
                Material = material;
            }

            if (vertFilePath != "")
            {
                InitializeVertices(vertFilePath);
            }
        }

        public void UpdateResources(Texture2D texture = null, Materials.AbstractMaterial material = null, string vertFilePath = "")
        {
            if (texture != null)
            {
                Texture?.Dispose();
                TextureView?.Dispose();
                Texture = texture;
                TextureView = new ShaderResourceView(Device, Texture);
            }

            if (material != null)
            {
                Material = material;
            }

            if (vertFilePath != "")
            {
                InitializeVertices(vertFilePath);
            }
        }

        public void UpdateResources(Texture2D[] cubetextures, Materials.AbstractMaterial material = null, string vertFilePath = "")
        {
            Texture?.Dispose();
            TextureView?.Dispose();
            TexturesCube = cubetextures;
            

            Texture2D texture2D = new Texture2D(Device, new Texture2DDescription
            {
                ArraySize = 6,
                MipLevels = 1,
                Height = 512,
                Width = 512,
                CpuAccessFlags = CpuAccessFlags.Read,
                Format = DXGI.Format.R32G32B32A32_UInt,
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Immutable //!!
            });

            isTextureCube = true;
           
            for(int i =0; i < 6; i++)
            {
                Device.ImmediateContext.CopySubresourceRegion(cubetextures[i], 0, null, texture2D, i);
                cubetextures[i].Dispose();
            }

            TextureView = new ShaderResourceView(Device, texture2D);

            if (material != null)
            {
                Material = material;
            }

            if (vertFilePath != "")
            {
                InitializeVertices(vertFilePath);
            }
        }

        public void UpdateResources(ShaderResourceView texture = null, Materials.AbstractMaterial material = null, string vertFilePath = "")
        {
            if (texture != null)
            {
                TextureView?.Dispose();

                TextureView = texture;
            }

            if (material != null)
            {
                Material = material;
            }

            if (vertFilePath != "")
            {
                InitializeVertices(vertFilePath);
            }
        }

        public void UpdateResources(string texturePath = "", Materials.AbstractMaterial material = null, string vertFilePath = "")
        {
            if (texturePath != "")
            {
                Texture?.Dispose();
                TextureView?.Dispose();
                Texture = TextureLoader.CreateTexture2DFromBitmap(Device, TextureLoader.LoadBitmap(new ImagingFactory2(), texturePath));
                TextureView = new ShaderResourceView(Device, Texture);
            }

            if (material != null)
            {
                Material = material;
            }

            if (vertFilePath != "")
            {
                InitializeVertices(vertFilePath);
            }
        }

        public void ChangeShaders(string pathVertShader, string pathPixShader)
        {
            MainShaderManager.ChangeShaders(pathVertShader, pathPixShader);
        }

        public void Draw(DeviceContext deviceContext, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilView,
            Matrix proj, Matrix view,
            Vector4 lightPos, Vector4 lightColor, Vector4 eyePos, float lightIntens,
            PrimitiveTopology topology, bool withShadowMap = true)
        {
            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            if (withShadowMap)
            {
                MainShaderManager.ShadowTransform = ShadowTransform;
            }

            MainShaderManager.Render(deviceContext, renderTargetViews, depthStencilView, 
                                     view, proj, Transform,
                                     lightPos, lightColor, eyePos, lightIntens, 
                                     Material, TextureView, isTextureCube, isLighten,
                                     topology, VerticesCount, 
                                     withShadowMap);
        }

        public void DrawShadowMap(DeviceContext deviceContext, RenderTargetView[] renderTargetViews, DepthStencilView depthStencilView,
            Matrix lightProj, Matrix lightView, Matrix lightShadowTransform,
            PrimitiveTopology topology)
        {
            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            ShadowTransform = lightShadowTransform;

            ShadowShaderManager.RenderShadow(deviceContext, renderTargetViews, depthStencilView, lightView, lightProj, lightShadowTransform, Transform, topology, VerticesCount);
        }

        public TrianglePositionNormalTextureAdjInput[] GenerateTriangleAdjForShadowVolume(TriangleInput[] trianglesPositionNormalTexture)
        {
            var ret = new List<TrianglePositionNormalTextureAdjInput>();
            foreach (var trg0 in trianglesPositionNormalTexture)
            {
                var trgAdj = new TrianglePositionNormalTextureAdjInput
                {
                    Point0 = trg0.Point0,
                    Point2 = trg0.Point1,
                    Point4 = trg0.Point2
                };

                foreach (var trg1 in trianglesPositionNormalTexture)
                {
                    var vrt0Comp0 = (trg0.Point0.Position == trg1.Point0.Position) || (trg0.Point0.Position == trg1.Point1.Position) || (trg0.Point0.Position == trg1.Point2.Position);
                    var vrt0Comp1 = (trg0.Point1.Position == trg1.Point0.Position) || (trg0.Point1.Position == trg1.Point1.Position) || (trg0.Point1.Position == trg1.Point2.Position);
                    var vrt0Comp2 = (trg0.Point2.Position == trg1.Point0.Position) || (trg0.Point2.Position == trg1.Point1.Position) || (trg0.Point2.Position == trg1.Point2.Position);

                    if (vrt0Comp1 || vrt0Comp0 || vrt0Comp2)
                    {
                        if (!(vrt0Comp0 && vrt0Comp1 && vrt0Comp2))
                        {
                            if (vrt0Comp0 && vrt0Comp1)
                            {
                                var vrt1Comp0 = (trg1.Point0.Position == trg0.Point0.Position) || (trg1.Point0.Position == trg0.Point1.Position);
                                var vrt1Comp1 = (trg1.Point1.Position == trg0.Point0.Position) || (trg1.Point1.Position == trg0.Point1.Position);
                                var vrt1Comp2 = (trg1.Point2.Position == trg0.Point0.Position) || (trg1.Point2.Position == trg0.Point1.Position);

                                if (vrt1Comp0 && vrt1Comp1)
                                    trgAdj.Point1 = trg1.Point2;
                                else if (vrt1Comp1 && vrt1Comp2)
                                    trgAdj.Point1 = trg1.Point0;
                                else if (vrt1Comp2 && vrt1Comp0)
                                    trgAdj.Point1 = trg1.Point1;
                            }
                            else if (vrt0Comp1 && vrt0Comp2)
                            {
                                var vrt2Comp0 = (trg1.Point0.Position == trg0.Point1.Position) || (trg1.Point0.Position == trg0.Point2.Position);
                                var vrt2Comp1 = (trg1.Point1.Position == trg0.Point1.Position) || (trg1.Point1.Position == trg0.Point2.Position);
                                var vrt2Comp3 = (trg1.Point2.Position == trg0.Point1.Position) || (trg1.Point2.Position == trg0.Point2.Position);

                                if (vrt2Comp0 && vrt2Comp1)
                                    trgAdj.Point3 = trg1.Point2;
                                else if (vrt2Comp1 && vrt2Comp3)
                                    trgAdj.Point3 = trg1.Point0;
                                else if (vrt2Comp3 && vrt2Comp0)
                                    trgAdj.Point3 = trg1.Point1;
                            }
                            else if (vrt0Comp0 && vrt0Comp2)
                            {
                                var vrt2Comp0 = (trg1.Point0.Position == trg0.Point2.Position) || (trg1.Point0.Position == trg0.Point0.Position);
                                var vrt2Comp1 = (trg1.Point1.Position == trg0.Point2.Position) || (trg1.Point1.Position == trg0.Point0.Position);
                                var vrt2Comp2 = (trg1.Point2.Position == trg0.Point2.Position) || (trg1.Point2.Position == trg0.Point0.Position);

                                if (vrt2Comp0 && vrt2Comp1)
                                    trgAdj.Point5 = trg1.Point2;
                                else if (vrt2Comp1 && vrt2Comp2)
                                    trgAdj.Point5 = trg1.Point0;
                                else if (vrt2Comp2 && vrt2Comp0)
                                    trgAdj.Point5 = trg1.Point1;
                            }
                        }
                    }
                }

                ret.Add(trgAdj);
            }

            return ret.ToArray();
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            Sampler.Dispose();
            Texture.Dispose();
            TextureView.Dispose();
            MatricesBuf.Dispose();
            MaterialBuf.Dispose();
            ShadowWorldBuffer.Dispose();
        }

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

        public struct FlagsBufferStruct
        {
            public bool IsCubeTexture;
        }

    }
}