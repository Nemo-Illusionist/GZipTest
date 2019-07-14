namespace GZipLib.Queue
{
    public interface IQueueAdding
    {
        void Add(long position, byte[] bytes);
    }
}