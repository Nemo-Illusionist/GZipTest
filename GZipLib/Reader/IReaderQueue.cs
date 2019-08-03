using GZipLib.Queue;

namespace GZipLib.Reader
{
    public interface IReaderQueue : IQueueCount, IQueueNextEvent, IQueueAdding
    {
    }
}