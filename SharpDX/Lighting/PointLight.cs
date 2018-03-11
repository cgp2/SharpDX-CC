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
using System.Windows.Forms;
using SharpDX.Components;
using System.IO;

namespace SharpDX.Lighting
{
    class PointLight : AbstractLight
    {
        public const int ShadowCubeMapSize = 2048;
        public Matrix Projection;
        public Matrix[] View = new Matrix[6];

        public PointLight(Vector4 position, Color4 color, float intensity)
        {
            WorldPosition = position;
            Color = color;
            Intensity = intensity;

            Projection = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(90.0f), 1.0f, 0.01f, 10);

            View[0] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.Right, Vector3.Up);
            View[1] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.Left, Vector3.Up);
            View[2] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.Up, Vector3.BackwardRH);
            View[3] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.Down, Vector3.ForwardRH);
            View[4] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.BackwardLH, Vector3.Up);
            View[5] = Matrix.LookAtLH( (Vector3)WorldPosition, (Vector3)WorldPosition + Vector3.ForwardLH, Vector3.Up);

            var texDesc = new Texture2DDescription
            {
                Width = ShadowCubeMapSize,
                Height = ShadowCubeMapSize,
                MipLevels = 1,
                ArraySize = 6,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.TextureCube
            };

            //graphics.SetViewport(0f, 0f, (float)CommonLight.SHADOW_CUBE_MAP_SIZE, (float)CommonLight.SHADOW_CUBE_MAP_SIZE);
            //graphics.SetRenderTargets((DepthStencilBuffer)light.ShadowMap);
            //graphics.Clear((DepthStencilBuffer)light.ShadowMap, SharpDX.Direct3D11.DepthStencilClearFlags.Depth, 1f, 0);

            //_cubemapDepthResolver.Parameters["View"].SetValue(((OmnidirectionalLight)light).GetCubemapView());
            //_cubemapDepthResolver.Parameters["Projection"].SetValue(((OmnidirectionalLight)light).GetCubemapProjection());

            //scene.RenderScene(gameTime, _cubemapDepthResolver, false, 0);
        }
    }
}
