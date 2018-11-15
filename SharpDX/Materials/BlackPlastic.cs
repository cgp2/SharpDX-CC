using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Materials
{
    class BlackPlastic : AbstractMaterial
    {
        public BlackPlastic()
        {
            Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
            Absorption = new Vector3(0.5f, 0.5f, 0.5f);
            Ambient = new Vector3(0f, 0f, 0f) ;
            Shiness = 2f;
        }
    }
}
