namespace GZipLib.Reader
{
    public interface IReaderQueue : IMore
    {
        ReadingPart Next();
    }

    public interface IMore
    {
        bool More(long position);
    }
}