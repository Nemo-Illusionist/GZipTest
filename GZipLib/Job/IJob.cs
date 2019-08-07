using System;

namespace GZipLib.Job
{
    public interface IJob : IDisposable
    {
        void Start();
        bool IsAlive();
        void Cancel();
        void Join();
    }
}