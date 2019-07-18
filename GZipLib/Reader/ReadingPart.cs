namespace GZipLib.Reader
{
    public class ReadingPart
    {
        public int Index { get; }
        public byte[] Data { get; }

        public ReadingPart(int index, byte[] data)
        {
            Index = index;
            Data = data;
        }
    }
}