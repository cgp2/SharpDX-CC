using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FbxNative;

using SharpDX.Direct3D11;

namespace SharpDX.Components
{
    internal class FBXComponent : AbstractComponent
    {
        private Scene CompScene;
        public FBXComponent(Device device) : base(device)
        {
        }

           
        protected override void InitializeVertices(string filePath)
        {
            FbxLoader fbxloader = new FbxLoader();
            CompScene = fbxloader.LoadScene(filePath);

            Vertices = MeshvertexToVPNT(CompScene.Meshes[0].Vertices);
            VerticesCount = Vertices.Length;
            // CompScene = fbxloader.LoadScene();
        }

        VertexPositionNormalTexture[] MeshvertexToVPNT(List<MeshVertex> meshVertecies)
        {
            List<VertexPositionNormalTexture> ret = new List<VertexPositionNormalTexture>();

            foreach(MeshVertex vrt in meshVertecies)
            {
                VertexPositionNormalTexture vpnt = new VertexPositionNormalTexture();
                vpnt.Position = new Vector4(vrt.Position, 1.0f);
                vpnt.Normal = new Vector4(vrt.Normal, 0.0f);
                vpnt.Texture = vrt.TexCoord0;

                ret.Add(vpnt);
            }

            return ret.ToArray();
        }

    }
}
