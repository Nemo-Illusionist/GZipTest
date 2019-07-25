using System;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public interface IWriterQueue : IQueueManager, IQueueAdding, IDisposable
    {
        void Start(INextCheck nextCheck);
    }
}
