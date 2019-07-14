using System;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public interface IWriterQueue : IQueueAdding
    {
        event EventHandler Event;

        void Start();
        void Cancel();
        void Join();
    }
}