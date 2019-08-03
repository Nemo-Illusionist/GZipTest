using System.IO.Compression;

namespace GZipLib.Reader
{
    public interface IReaderJobFactory
    {
        IReaderJob Create(IReaderQueue queue, CompressionMode mode);
    }
}