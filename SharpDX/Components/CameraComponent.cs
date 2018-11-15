using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics;
using SharpDX.DXGI;

namespace SharpDX
{
    public class CameraComponent
    {
        public Matrix View, Proj, RotMatrix;
        public Vector3 EyePosition, TargetPosition, UpVector;
        public float Fov, Aspect;

        private float yaw, pitch;

        public CameraComponent(float aspect, Vector3 pos, float yaw, float pitch)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            var yawRad = MathUtil.DegreesToRadians(this.yaw);
            var pitchRad = MathUtil.DegreesToRadians(this.pitch);
            RotMatrix = Matrix.RotationYawPitchRoll(yawRad, pitchRad, MathUtil.DegreesToRadians(0));
            EyePosition = pos;
            TargetPosition = RotMatrix.Forward + EyePosition;
            UpVector = RotMatrix.Up;
            this.Aspect = aspect;

            UpdateViewMatrix();
            Proj = Matrix.PerspectiveFovLH((float)Math.PI / 2.0f, aspect, 0.1f, 1000.0f);
        }

        public Matrix World2Proj(Matrix worldCoord)
        {
            Matrix projCoord = worldCoord * View * Proj;
            return projCoord;
        }

        public Vector3 ChangeTargetPosition(float yaw, float pitch)
        {
            this.yaw += 0.04f * yaw;
            this.pitch += 0.04f * pitch;
            var yawRad = MathUtil.DegreesToRadians(this.yaw);
            var pitchRad = MathUtil.DegreesToRadians(this.pitch);

            RotMatrix = Matrix.RotationYawPitchRoll(yawRad, pitchRad, MathUtil.DegreesToRadians(0));
            TargetPosition  = RotMatrix.Forward + EyePosition;
            UpVector = RotMatrix.Up;
            UpdateViewMatrix();
            return TargetPosition;
        }

        private Matrix UpdateViewMatrix()
        {
            View = Matrix.LookAtLH(EyePosition, TargetPosition, UpVector);
            return View;
        }

        public void MoveForward()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Forward, 1.3f);
            EyePosition += scale;
            TargetPosition += scale;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveBackward()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Backward, 1.3f);
            EyePosition += scale;
            TargetPosition += scale;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveRight()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Left, 1.3f);
            EyePosition += scale;
            TargetPosition += scale;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveDown()
        {
            EyePosition.Y -= 1.3f;
            TargetPosition.Y -= 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveUp()
        {
            EyePosition.Y += 1.3f;
            TargetPosition.Y += 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveLeft()
        {
            Vector3 scale = Vector3.Multiply(RotMatrix.Right, 1.3f);
            EyePosition += scale;
            TargetPosition += scale;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveForwardAxis()
        {
            EyePosition.Z += 1.3f;
            TargetPosition.Z += 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveBackwardAxis()
        {
            EyePosition.Z -= 1.3f;
            TargetPosition.Z -= 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveRightAxis()
        {
            EyePosition.X += 1.3f;
            TargetPosition.X += 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

        public void MoveLeftAxis()
        {
            EyePosition.X -= 1.3f;
            TargetPosition.X -= 1.3f;

            View = Matrix.LookAtLH(EyePosition, TargetPosition, RotMatrix.Up);
        }

    }
}
