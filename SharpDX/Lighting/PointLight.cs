using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX.Lighting
{
    class PointLight : AbstractLight
    {
        public PointLight(Vector4 position, Color4 color, float intensity)
        {
            WorldPosition = position;
            Color = color;
            Intensity = intensity;
        }
    }
}
