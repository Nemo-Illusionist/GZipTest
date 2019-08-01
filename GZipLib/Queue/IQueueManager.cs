namespace GZipLib.Queue
{
    public interface IQueueManager
    {
        void Cancel();
        void Join();
    }
}
