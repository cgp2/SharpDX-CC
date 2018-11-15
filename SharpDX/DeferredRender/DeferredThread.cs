using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Components;
using System.Threading;
using SharpDX.Direct3D11;

namespace SharpDX.DeferredRender
{
    internal class DeferredThread
    {
        private AbstractComponent[] components;
        private Thread mainThread;
        public DeviceContext Context;

        public DeferredThread(AbstractComponent[] components, DeviceContext context)
        {
            Context = context;
            this.components = components;

            mainThread = new Thread(THREADMAIN);
        }

        //Переделать как пойму что нужно 
        private static void THREADMAIN(object x)
        {
            throw new NotImplementedException();
        }
    }
}
