namespace GZipLib.Writer
{
    public interface IWritelQueue
    {
        void Add(long position, byte[] bytes);
    }
}