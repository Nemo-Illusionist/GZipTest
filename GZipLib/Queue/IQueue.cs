using GZipLib.Reader;
using GZipLib.Writer;

namespace GZipLib.Queue
{
    public interface IQueue : IReaderQueue, IWriterQueue
    {
    }
}