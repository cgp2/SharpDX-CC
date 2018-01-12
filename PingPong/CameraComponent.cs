using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics;
using SharpDX.DXGI;

namespace PingPong
{
    public class CameraComponent
    {
        public Matrix View;
        public Matrix Proj;
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
            Proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, aspect, 0.1f, 100.0f);
        }

        public Matrix World2Proj(Matrix worldCoord)
        {
            Matrix projCoord = worldCoord * View * Proj;
            return projCoord;
        }

        public Vector3 ChangeTargetPosition(float yaw, float pitch)
        {
            var lookAt = Vector3.UnitZ;
            var yawRad = MathUtil.DegreesToRadians(0.0001f*yaw);
            var pitchRad = MathUtil.DegreesToRadians(0.0001f * pitch);

            var rotMatr = Matrix.RotationYawPitchRoll(yawRad, pitchRad, MathUtil.DegreesToRadians(0));
            //rotMatr.Transpose();
            lookAt = rotMatr.Forward + target;
            target = (Vector3)Vector3.Transform(lookAt, rotMatr);

            //target = rotMatr.Forward;
            up = rotMatr.Up;
            UpdateViewMatrix();
            return target;
        }

        private Matrix UpdateViewMatrix()
        {
            View = Matrix.LookAtRH(EyePosition, target, up);
            return View;
        }

        public void MoveForward()
        {
            EyePosition.Z += 0.3f;
            target.Z += 0.3f;
            View = Matrix.LookAtLH(EyePosition, target, Vector3.UnitY);
        }

        public void MoveBackward()
        {
            EyePosition.Z -= 0.3f;
            target.Z -= 0.3f;
            View = Matrix.LookAtLH(EyePosition, target, Vector3.UnitY);
        }

        public void MoveRight()
        {
            EyePosition.X += 0.3f;
            target.X += 0.3f;
            View = Matrix.LookAtLH(EyePosition, target, Vector3.UnitY);
        }

        public void MoveLeft()
        {
            EyePosition.X -= 0.3f;
            target.X -= 0.3f;
            View = Matrix.LookAtLH(EyePosition, target, Vector3.UnitY);
        }

        //public void ChangeTargetPosition(float yaw, float pitch)
        //{
        //    var lookAt = Vector3.UnitZ;

        //    this.yaw += yaw;
        //    this.pitch += pitch;

        //    var yawRad = MathUtil.DegreesToRadians(0.0001f * yaw);
        //    var pitchRad = MathUtil.DegreesToRadians(0.0001f * pitch);
        //    float rollRad = 0;
        //    var rotMatr = Matrix3x3.RotationYawPitchRoll(yawRad, pitchRad, rollRad);

        //    target = Vector3.Transform(lookAt, rotMatr);
        //    //Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotMatr);
        //    //rotMatr.Transpose();
        //    //target = rotMatr.Forward;

        //    //WTF
        //    //lookAt = (Vector3)Vector3.Transform(lookAt, rotMatr) + target;
        //    //target = (Vector3)Vector3.Transform(lookAt, rotMatr);

        //    //target = rotMatr * lookAt;
        //    //Matrix.RotationYawPitchRoll(yawRad, pitchRad, rollRad);
        //    //UpdateViewMatrix();
        //    UpdateViewMatrix();
        //    //return target;
        //    //var yawRad = MathUtil.DegreesToRadians(yaw);
        //    //var lookAt = Vector3.UnitZ;
        //    //Matrix rotMAtr = Matrix.RotationY(yawRad);

        //    //var transformRef = Vector3.Transform(lookAt, rotMAtr);
        //    //Vector3 caml = target + (Vector3)transformRef;

        //    //view = Matrix.LookAtLH(position, caml, new Vector3(0.0f, 1.0f, 0.0f));
        //}


    }
}
