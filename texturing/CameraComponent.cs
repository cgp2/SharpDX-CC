using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics;
using SharpDX.DXGI;
using Matrix = SharpDX.Matrix;
using Vector3 = SharpDX.Vector3;
using MathUtil = SharpDX.MathUtil;

namespace texturing
{
    public class CameraComponent
    {
        public Matrix View, Proj, RotMatrix;
        public Vector3 EyePosition, target, up;
        public float FOV, aspect;

        private float yaw = 0, pitch = 0;


        public CameraComponent(float aspect, Vector3 pos, Vector3 trgt)
        {
            EyePosition = pos;
            target = trgt;
            up = Vector3.UnitY;
            this.aspect = aspect;

            View = Matrix.LookAtLH(pos, trgt, up);
            Proj = Matrix.PerspectiveFovLH((float)Math.PI / 2.0f, aspect, 0.1f, 100.0f);
            RotMatrix = Matrix.RotationYawPitchRoll(MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(0));
        }

        public Matrix World2Proj(Matrix worldCoord)
        {
            Matrix projCoord = worldCoord * View * Proj;
            return projCoord;
        }

        public Vector3 ChangeTargetPosition(float yaw, float pitch)
        {
            var yawRad = MathUtil.DegreesToRadians(0.04f*yaw);
            var pitchRad = MathUtil.DegreesToRadians(0.04f * pitch);

            RotMatrix = Matrix.RotationYawPitchRoll(yawRad, pitchRad, MathUtil.DegreesToRadians(0));
            target  = RotMatrix.Forward + EyePosition;
            up = RotMatrix.Up;
            UpdateViewMatrix();
            return target;
        }

        private Matrix UpdateViewMatrix()
        {
            View = Matrix.LookAtLH(EyePosition, target, up);
            return View;
        }

        public void MoveForward()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Forward, -1.3f);
            EyePosition += scale;
            target += scale;

            View = Matrix.LookAtLH(EyePosition, target, RotMatrix.Up);
        }

        public void MoveBackward()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Backward, -1.3f);
            EyePosition += scale;
            target += scale;

            View = Matrix.LookAtLH(EyePosition, target, RotMatrix.Up);
        }

        public void MoveRight()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Right, 1.3f);
            EyePosition += scale;
            target += scale;

            View = Matrix.LookAtLH(EyePosition, target, RotMatrix.Up);
        }

        public void MoveLeft()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Left, 1.3f);
            EyePosition += scale;
            target += scale;

            View = Matrix.LookAtLH(EyePosition, target, RotMatrix.Up);
        }

    }
}
