using System;

namespace GZipLib.Queue
{
    public interface IQueueManager
    {
        event EventHandler EndQueueEvent;
        
        void Cancel();
        void Join();
    }
}
