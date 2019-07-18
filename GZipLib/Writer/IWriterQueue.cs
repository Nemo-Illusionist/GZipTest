using System;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public interface IWriterQueue : IQueueAdding, IDisposable
    {
        event EventHandler EndWriterQueueEvent;

        void Start(INextCheck nextCheck);
        void Cancel();
        void Join();
    }
}
