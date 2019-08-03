using System;

namespace GZipLib.Queue
{
    public interface IQueueAddingEvent
    {
        event EventHandler AddEvent;
    }
}