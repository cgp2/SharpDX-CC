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
    class SpotLight : AbstractLight
    {
        public Matrix ShadowTransform;
        public Matrix Projection;
        public Matrix View;
        public Vector4 Target;
        public float Attention;
        public float FOV;
        private float yaw;
        private float pitch;


        public SpotLight(Vector4 position, float yaw, float pitch, float attention, float fov, Color4 color, float intensity)
        {
            WorldPosition = position;
            Color = color;
            Intensity = intensity;
            this.yaw = yaw;
            this.pitch = pitch;
            this.FOV = fov;
            this.Attention = attention;

            var CoordsTransform =  Matrix.AffineTransformation(1f, Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yaw), MathUtil.DegreesToRadians(pitch), 0f), (Vector3)position);
            //WorldPosition = Vector4.Transform(WorldPosition, CoordsTransform);

            Target = new Vector4(0, 0, 0, 1);
            var up = new Vector4(0, 1, 0, 0);


            View = Matrix.LookAtLH((Vector3)WorldPosition, (Vector3)Target, (Vector3)up);
            //Projection = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(fov), 1.0f, 1f, 100);
            Projection = Matrix.OrthoLH(16, 16, 0.1f, 1000);

            ShadowTransform = View * Projection;
        }
    }
}
