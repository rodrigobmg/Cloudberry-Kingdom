﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace CoreEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MyOwnVertexFormat : IVertexType //, IReadWrite
    {
        public Vector2 xy;
        public Vector2 uv;
        public Color Color;

        public MyOwnVertexFormat(Vector2 XY, Vector2 UV, Color color)
        {
            this.xy = XY;
            this.uv = UV;
            this.Color = color;
        }

        public MyOwnVertexFormat(Vector2 XY, Vector2 UV, Color color, Vector3 depth)
        {
            this.xy = XY;
            this.uv = UV;
            this.Color = color;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        /*
        static string[] _bits_to_save = new string[] { "xy", "uv", "Color" };
        public void WriteCode(string prefix, StreamWriter writer)
        {
            Tools.WriteFieldsToCode(this, prefix, writer, _bits_to_save);
        }
        public void Write(StreamWriter writer)
        {
            Tools.WriteFields(this, writer, _bits_to_save);
        }
        public void Read(StreamReader reader)
        {
            Tools.ReadFields(this, reader);
        }*/
    }
}
