namespace GZipLib.Settings
{
    public class CompressorSettings
    {
        public int PageSize { get; }
        public int BufferSize { get; }
        public int ThreadPoolCount { get; }

        public CompressorSettings(int pageSize, int bufferSize, int threadPoolCount)
        {
            PageSize = pageSize;
            BufferSize = bufferSize;
            ThreadPoolCount = threadPoolCount;
        }

        public CompressorSettings() : this(4194304, 81920, 10)
        {
        }
    }
}