using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Aardwolf.Render
{
    public class VAO : IDisposable
    {
        public int Handle { get; private set; }

        public VAO()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }

        public void EnableAttribute(int index)
        {
            Bind();
            GL.EnableVertexAttribArray(index);
        }

        public void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            Bind();
            GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
        }

        public void SetAttributes() 
        { 
            EnableAttribute(0); 
            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0); 
            EnableAttribute(1); 
            VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float)); 
            EnableAttribute(2); 
            VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float)); 
        }

        public void DisableAttribute(int index)
        {
            Bind();
            GL.DisableVertexAttribArray(index);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(Handle);
        }
    }

}

