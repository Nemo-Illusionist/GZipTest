namespace GZipLib.Reader
{
    public interface IReader
    {
        long Length { get; }
        byte[] Read(long position, int length);
    }
}