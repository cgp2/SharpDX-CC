using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace SharpDX.Lighting
{
    class AbstractLight 
    {
        public Vector4 WorldPosition;
        public Vector4 Color;
        public float Intensity;
    }
}
