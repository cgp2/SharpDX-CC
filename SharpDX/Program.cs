using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.IO;
using System.Threading;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;
using SharpDX.Mathematics;
using System.Linq;


namespace SharpDX
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Games.ShadowMapLightingGame())
            {
                game.Run();
            }
        }
    }
}
