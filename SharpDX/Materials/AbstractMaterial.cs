using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Materials
{
    public abstract class AbstractMaterial
    {
        public Vector3 Diffuse;
        public Vector3 Absorption;
        public Vector3 Ambient;
        public float Shiness;
    }
}
