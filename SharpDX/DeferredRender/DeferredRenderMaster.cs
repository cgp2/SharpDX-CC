using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Components;
using SharpDX.DeferredRender;
using SharpDX.Direct3D11;

namespace SharpDX
{
    internal class DeferredRenderMaster
    {
        private const int ThreadNum = 12;

        private AbstractComponent[] components;
        private DeferredThread[] threads;
        private DeviceContext[] contexts;

        public DeferredRenderMaster(AbstractComponent[] components)
        {
            threads = new DeferredThread[ThreadNum];

            contexts = new DeviceContext[ThreadNum];
     
            this.components = components;

            var remainder = components.Length % ThreadNum;

            var n = 0;
            for (int i = 0; i < ThreadNum; i++)
            {
                var input = new List<AbstractComponent>
                {
                    components[n++], components[n++]
                };
                if (remainder != 0)
                {
                    input.Add(components[components.Length - remainder]);
                    remainder--;
                }

                threads[i] = new DeferredThread(input.ToArray(), contexts[i]);
            }
        }


    }
    
}
