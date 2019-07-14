namespace GZipLib.Settings
{
    public class CompressorSettings
    {
        public int PageSize { get; }
        public int BufferSize { get; }
        public int ThreadPoolSize { get; }

        public CompressorSettings(int pageSize, int bufferSize, int threadPoolSize)
        {
            PageSize = pageSize;
            BufferSize = bufferSize;
            ThreadPoolSize = threadPoolSize;
        }

        public CompressorSettings() : this(4194304, 81920, 10)
        {
        }
    }
}