using System;
using GZipLib.Reader;

namespace GZipLib.Queue
{
    public interface IQueue : IReaderQueue, IWriterQueue, IDisposable
    {
    }
}