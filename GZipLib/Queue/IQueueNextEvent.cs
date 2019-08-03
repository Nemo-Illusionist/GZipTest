using System;

namespace GZipLib.Queue
{
    public interface IQueueNextEvent
    {
        event EventHandler NextEvent;
    }
}