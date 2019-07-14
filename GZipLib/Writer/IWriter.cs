using System;

namespace GZipLib.Writer
{
    public interface IWriter : IDisposable
    {
        void Write(byte[] data);
    }
}