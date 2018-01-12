﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX
{
   public struct VertexPositionColorTexture
    {
        public Vector3 Position;
        public Color4 Color;
        public Vector2 TextureCoordinates;

        public VertexPositionColorTexture(Vector3 position, Color4 color, Vector2 textureCoord)
        {
            Position = position;
            Color = color;
            TextureCoordinates = textureCoord;
        }
    }
}