using System;
using System.Collections.Generic;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.InputStructures;
using SharpDX.Materials;

namespace SharpDX.Components
{
    internal class ObjModelComponent : AbstractComponent
    {
        //private TrianglePositionNormalTextureStripInput[] inputShadowVolume;
        //public TrianglePositionNormalTextureAdjInput[] TrglsAdjInput;

        protected override void InitializeVertices(string filePath)
        {
            if (filePath.Length == 0) return;
            var triangles = new TriangleInput[0];
            Game.ObjLoader.LoadObjModel(Device, filePath, out VertexBuffer, out VerticesCount, out Vertices, out triangles);
        }

        //public override void Draw(DeviceContext deviceContext, Matrix proj, Matrix view)
        //{
        //    var quat = Quaternion.RotationMatrix(Rotation);
        //    Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

        //    var shdTr = Transform * ShadowTransform;

        //    deviceContext.PixelShader.SetShaderResource(0, TextureView);
        //    deviceContext.PixelShader.SetSampler(0, Sampler);

        //    //Material Buffer
        //    deviceContext.UpdateSubresource(ref MaterialBufStruct, MaterialBuf);
        //    deviceContext.PixelShader.SetConstantBuffer(0, MaterialBuf);

        //    //Matrix buffer
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

        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));
        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        //    deviceContext.Draw(VerticesCount, 0);
        //}


            //NEXT ITERATION OF REFACTOR
        //public override void DrawShadow(DeviceContext deviceContext, Matrix shadowTransform, Matrix lightProj, Matrix lightView)
        //{
        //    ShadowTransform = shadowTransform;

        //    var quat = Quaternion.RotationMatrix(Rotation);
        //    Matrix.AffineTransformation(Scaling.X, ref RotationCenter, ref quat, ref Translation, out Transform);

        //    //MAtrix buffer
        //    var matrixStruct = new MatrixBufferStruct
        //    {
        //        World = Transform,
        //        View = lightView,
        //        Proj = lightProj
        //    };
        //    deviceContext.UpdateSubresource(ref matrixStruct, ShadowWorldBuffer);
        //    deviceContext.VertexShader.SetConstantBuffer(0, ShadowWorldBuffer);

        //    deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexPositionNormalTexture>(), 0));

        //    deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        //    deviceContext.Draw(VerticesCount, 0);
        //}

        public ObjModelComponent(Device device) : base(device)
        {
        }
    }
}