using System.IO.Compression;

namespace GZipLib.Reader
{
    public interface IReaderQueueFactory
    {
        IReaderQueue Create(CompressionMode mode);
    }
}