using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Materials
{
    class SilverMaterial : AbstractMaterial
    {
        public SilverMaterial()
        {
            Diffuse = new Vector3(0.2775f, 0.2775f, 0.2775f);
            Absorption = new Vector3(0.773911f, 0.773911f, 0.773911f);
            Ambient = new Vector3(0.23125f, 0.23125f, 0.23125f) ;
            Shiness = 89.7f;
        }
    }
}
