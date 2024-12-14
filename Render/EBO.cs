using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Aardwolf.Render
{
    
    public class EBO : IDisposable
    {
        public int Handle { get; private set; }

        public EBO()
        {
            Handle = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
        }

        public void SetData(uint[] indices, BufferUsageHint usage)
        {
            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, usage);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }

}

