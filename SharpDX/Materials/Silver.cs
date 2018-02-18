using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Materials
{
    class Silver : AbstractMaterial
    {
        public Silver()
        {
            Diffuse = new Vector3(0.2775f, 0.2775f, 0.2775f);
            Absorption = new Vector3(0.773911f, 0.773911f, 0.773911f);
            Ambient = new Vector3(0.1f, 0.1f, 0.1f) ;
            Shiness = 89.7f;
        }
    }
}
