using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Aardwolf.Render
{
    public class VBO : IDisposable
    {
        public int Handle { get; private set; }

        public VBO()
        {
            Handle = GL.GenBuffer();
        }

        public void Bind(BufferTarget target = BufferTarget.ArrayBuffer)
        {
            GL.BindBuffer(target, Handle);
        }

        public void SetData<T>(T[] data, BufferUsageHint usage, BufferTarget target = BufferTarget.ArrayBuffer) where T : struct
        {
            Bind(target);
            GL.BufferData(target, data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), data, usage);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }

}
