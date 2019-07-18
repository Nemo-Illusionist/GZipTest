using System;

namespace GZipLib.Settings
{
    public class CompressorSettings
    {
        public int PageSize { get; }
        public int BufferSize { get; }
        public int ThreadPoolSize { get; }

        public CompressorSettings(int pageSize, int bufferSize, int threadPoolSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (threadPoolSize <= 0) throw new ArgumentOutOfRangeException(nameof(threadPoolSize));

            PageSize = pageSize;
            BufferSize = bufferSize;
            ThreadPoolSize = threadPoolSize;
        }

        public CompressorSettings() : this(4096 * 1024, 81920, 10)
        {
        }
    }
}
