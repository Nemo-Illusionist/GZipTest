using System;

namespace GZipLib.Job
{
    public interface IJob : IDisposable
    {
        void Cancel();
        void Join();
    }
}