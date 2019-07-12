namespace GZipLib.Reader
{
    public interface IReader
    {
        byte[] Read(int position, int length);
    }
}