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
    class DirectionalLight : AbstractLight
    {
        public Matrix ShadowTransform;
        public Matrix Projection;
        public Matrix View;
        public Vector4 Target;
        public float Attention;
        public float Fov;
        private float yaw;
        private float pitch;


        public DirectionalLight(Vector4 position, float widht, float height, Color4 color, float intensity)
        {
            WorldPosition = position;
            Color = color;
            Intensity = intensity;
       
            Target = new Vector4(0, 0, 0, 1);
            var up = new Vector4(0, 1, 0, 0);

            View = Matrix.LookAtLH((Vector3)WorldPosition, (Vector3)Target, (Vector3)up);
            //Projection = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(fov), 1.0f, 1f, 100);
            Projection = Matrix.OrthoLH(widht, height, 0.1f, 500);

            ShadowTransform = View * Projection;
        }
    }
}
