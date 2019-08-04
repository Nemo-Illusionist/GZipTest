using GZipLib.Core;

namespace GZipLib.Queue
{
    public interface IQueueNext
    {
        Part Next();
    }
}