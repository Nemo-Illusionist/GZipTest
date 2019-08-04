using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class ReaderJob : BaseReaderJob
    {
        public ReaderJob(IReader reader, IReaderQueue queue, CompressorSettings settings)
            : base(reader, queue, settings)
        {
        }

        protected override byte[] Read()
        {
            var length = Reader.LeftBytes < Settings.PageSize ? (int) Reader.LeftBytes : Settings.PageSize;
            return Reader.Read(length);
        }
    }
}