using System.Collections.Generic;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.InputStructures;
using SharpDX.Materials;
using SharpDX.WIC;

namespace SharpDX.Components
{
    internal class ObjModelComponent : AbstractComponent
    {
        private TrianglePositionNormalTextureStripInput[] inputShadowVolume;

        public ObjModelComponent(Device device, string objFilePath, string texturePath)
        {
            Device = device;

            var objLoader = new ObjLoader();
            var triangles = new TrianglePositionNormalTextureInput[0];
            objLoader.LoadObjModel(device, objFilePath, out VertexBuffer, out VerticesCount, out Vertices, out triangles);

            var r = GenerateTriangleAdjForShadowVolume(triangles);

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, Vertices);

            Texture = TextureLoader.CreateTexture2DFromBitmap(device,
                TextureLoader.LoadBitmap(new ImagingFactory2(), texturePath));
            TextureView = new ShaderResourceView(device, Texture);

            SamplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear
            };
            Sampler = new SamplerState(device, SamplerStateDescription);


            Material = new Silver();

            InitializeResources();
        }

        public TrianglePositionNormalTextureAdjInput[] GenerateTriangleAdjForShadowVolume(TrianglePositionNormalTextureInput[] trianglesPositionNormalTexture)
        {
            List<TrianglePositionNormalTextureAdjInput> ret = new List<TrianglePositionNormalTextureAdjInput>();
            foreach (var trg1 in trianglesPositionNormalTexture)
            {
                var trgAdj = new TrianglePositionNormalTextureAdjInput
                {
                    Point2 = trg1.Point1,
                    Point4 = trg1.Point2,
                    Point6 = trg1.Point3
                };

                foreach (var trg2 in trianglesPositionNormalTexture)
                {
                    var vrt1Comp1 = (trg1.Point1.Position == trg2.Point1.Position) || (trg1.Point1.Position == trg2.Point2.Position) || (trg1.Point1.Position == trg2.Point3.Position);
                    var vrt1Comp2 = (trg1.Point2.Position == trg2.Point1.Position) || (trg1.Point2.Position == trg2.Point2.Position) || (trg1.Point2.Position == trg2.Point3.Position);
                    var vrt1Comp3 = (trg1.Point3.Position == trg2.Point1.Position) || (trg1.Point3.Position == trg2.Point2.Position) || (trg1.Point3.Position == trg2.Point3.Position);

                    if (vrt1Comp1 && vrt1Comp2)
                    {
                        var vrt2Comp1 = (trg2.Point1.Position == trg1.Point1.Position) || (trg2.Point1.Position == trg1.Point2.Position);
                        var vrt2Comp2 = (trg2.Point2.Position == trg1.Point1.Position) || (trg2.Point2.Position == trg1.Point2.Position);
                        var vrt2Comp3 = (trg2.Point3.Position == trg1.Point1.Position) || (trg2.Point3.Position == trg1.Point2.Position);

                        if (vrt2Comp1 && vrt2Comp2)
                            trgAdj.Point3 = trg2.Point3;
                        else if (vrt2Comp2 && vrt2Comp3)
                            trgAdj.Point3 = trg2.Point1;
                        else if (vrt2Comp3 && vrt2Comp1)
                            trgAdj.Point3 = trg2.Point2;
                    }
                    else if (vrt1Comp2 && vrt1Comp3)
                    {
                        var vrt2Comp1 = (trg2.Point1.Position == trg1.Point2.Position) || (trg2.Point1.Position == trg1.Point3.Position);
                        var vrt2Comp2 = (trg2.Point2.Position == trg1.Point2.Position) || (trg2.Point2.Position == trg1.Point3.Position);
                        var vrt2Comp3 = (trg2.Point3.Position == trg1.Point2.Position) || (trg2.Point3.Position == trg1.Point3.Position);

                        if (vrt2Comp1 && vrt2Comp2)
                            trgAdj.Point5 = trg2.Point3;
                        else if (vrt2Comp2 && vrt2Comp3)
                            trgAdj.Point5 = trg2.Point1;
                        else if (vrt2Comp3 && vrt2Comp1)
                            trgAdj.Point5 = trg2.Point2;
                    }
                    else if (vrt1Comp1 && vrt1Comp3)
                    {
                        var vrt2Comp1 = (trg2.Point1.Position == trg1.Point3.Position) || (trg2.Point1.Position == trg1.Point1.Position);
                        var vrt2Comp2 = (trg2.Point2.Position == trg1.Point3.Position) || (trg2.Point2.Position == trg1.Point1.Position);
                        var vrt2Comp3 = (trg2.Point3.Position == trg1.Point3.Position) || (trg2.Point3.Position == trg1.Point1.Position);

                        if (vrt2Comp1 && vrt2Comp2)
                            trgAdj.Point1 = trg2.Point3;
                        else if (vrt2Comp2 && vrt2Comp3)
                            trgAdj.Point1 = trg2.Point1;
                        else if (vrt2Comp3 && vrt2Comp1)
                            trgAdj.Point1 = trg2.Point2;
                    }
                }

                ret.Add(trgAdj);
            }

            return ret.ToArray();
        }

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

        public override void Update()
        {
            Vertices = Transformation(Vertices, InitialPosition, Rotation);
            VertexBuffer = Buffer.Create(Device, BindFlags.VertexBuffer, Vertices);
        }

        public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        {
            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            var shdTr = Transform * ShadowTransform;

            deviceContext.PixelShader.SetShaderResource(0, TextureView);
            deviceContext.PixelShader.SetSampler(0, Sampler);

            //Material Buffer
            deviceContext.UpdateSubresource(ref MaterialBufStruct, MaterialBuf);
            deviceContext.PixelShader.SetConstantBuffer(0, MaterialBuf);

            //Matrix buffer
            var matrixStruct = new MatrixBufferStruct
            {
                World = Transform,
                View = view,
                Proj = proj
            };
            deviceContext.UpdateSubresource(ref matrixStruct, MatricesBuf);
            deviceContext.VertexShader.SetConstantBuffer(3, MatricesBuf);

            //ShadowBuf
            deviceContext.UpdateSubresource(ref shdTr, ShadowTransformBuffer);
            deviceContext.VertexShader.SetConstantBuffer(4, ShadowTransformBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            deviceContext.Draw(VerticesCount, 0);
        }

        public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView)
        {
            ShadowTransform = shadowTransform;

            var quat = Quaternion.RotationMatrix(Rotation);
            Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

            //MAtrix buffer
            var matrixStruct = new MatrixBufferStruct
            {
                World = Transform,
                View = lightView,
                Proj = lightProj
            };
            deviceContext.UpdateSubresource(ref matrixStruct, ShadowWorldBuffer);
            deviceContext.VertexShader.SetConstantBuffer(0, ShadowWorldBuffer);

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(VerticesCount, 0);
        }

        public override void Dispose()
        {
            VertexBuffer.Dispose();
            Sampler.Dispose();
            Texture.Dispose();
            TextureView.Dispose();
            MatricesBuf.Dispose();
            MaterialBuf.Dispose();
            ShadowWorldBuffer.Dispose();
            ConstantBuffer.Dispose();
        }
    }
}