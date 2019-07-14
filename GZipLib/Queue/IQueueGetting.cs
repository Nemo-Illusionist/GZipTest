namespace GZipLib.Queue
{
    public interface IQueueGetting 
    {
        (long position, int length, int index)? Next();
    }
}