using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class ReaderQueue : BaseReaderQueue
    {
        public ReaderQueue(IReader reader, CompressorSettings settings) : base(reader, settings)
        {
        }
        
        protected override byte[] Read()
        {
            var length = Reader.LeftBytes < Settings.PageSize ? (int) Reader.LeftBytes : Settings.PageSize;
            return Reader.Read(length);
        }
    }
}
