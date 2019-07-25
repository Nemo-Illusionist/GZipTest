using System;
using GZipLib.Queue;

namespace GZipLib.Reader
{
    public interface IReaderQueue : IQueueManager, IQueueNext, INextCheck, IDisposable
    {
        void Start();
    }
}
