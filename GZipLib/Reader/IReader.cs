using System;

namespace GZipLib.Reader
{
    public interface IReader : IDisposable
    {
        long Length { get; }
        byte[] Read(int length);
        byte Read();
    }
}
