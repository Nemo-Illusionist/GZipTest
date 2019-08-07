using GZipLib.Queue;

namespace GZipLib.Writer
{
    public interface IWriterQueue : IQueueNext, IQueueAddingEvent
    {
    }
}