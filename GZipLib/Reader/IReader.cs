using System;

namespace GZipLib.Reader
{
    public interface IReader : IDisposable
    {
        long Length { get; }
        byte[] Read(long position, int length);
        byte Read(long position);
    }
}
