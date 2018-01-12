using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.IO;

namespace SharpDX
{
    class FileReader
    {
        public static Vector4[] ReadObj(string path)
        {
            System.IO.FileStream objFile = new System.IO.FileStream(path, System.IO.FileMode.Open);



            Vector4[] ret = new Vector4[2];

            return ret;
        }
    }
}
