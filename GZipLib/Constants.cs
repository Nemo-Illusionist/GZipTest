namespace GZipLib
{
    internal static class Constants
    {
        internal static readonly byte[] DefaultHeader = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00};

        internal const int PageSize = 4096 * 1024;
        internal const int BufferSize = 81920;
        internal const int ThreadPoolSize = 10;
        internal const bool ReaderQueueWithCache = false;

        internal const int ModIndex = 0;
        internal const int InputIndex = 1;
        internal const int OutputIndex = 2;
    }
}