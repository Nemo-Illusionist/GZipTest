using System;
using GZipLib.Queue;

namespace GZipLib.Reader
{
    public interface IReaderQueue : IDisposable, IQueueNext, INextCheck
    {
    }
}
