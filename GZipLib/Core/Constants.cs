namespace GZipLib.Core
{
    internal static class Constants
    {
        internal const int DefaultGzipHeaderLength = 10;

        internal const int PageSize = 4096 * 1024;
        internal const int BufferSize = 81920;
        internal const int ThreadPoolSize = 10;

        internal const int ModIndex = 0;
        internal const int InputIndex = 1;
        internal const int OutputIndex = 2;
    }
}
