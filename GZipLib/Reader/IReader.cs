using System;

namespace GZipLib.Reader
{
    public interface IReader : IDisposable
    {
        long LeftBytes { get; }
        
        byte[] Read(int length);
        byte Read();
    }
}
