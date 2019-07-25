using System;

namespace GZipLib.Settings
{
    [Serializable]
    public class CompressorSettings
    {
        public int PageSize { get; set; }
        public int BufferSize { get; set; }
        public int ThreadPoolSize { get; set; }

        public int OutputIndex { get; set; }
        public int InputIndex { get; set; }
        public int ModIndex { get; set; }
    }
}
