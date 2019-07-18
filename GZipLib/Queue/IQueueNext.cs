using GZipLib.Reader;

namespace GZipLib.Queue
{
    public interface IQueueNext
    {
        ReadingPart Next();
    }
}