using System.IO.Compression;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public interface IWriterJobFactory
    {
        IWriterJob Create(IWriterQueue queue, CompressionMode mode);
    }
}