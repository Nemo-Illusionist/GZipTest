using System;
using System.Collections.Generic;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class ReaderQueue : BaseReaderQueue
    {
        public ReaderQueue(IReader reader, CompressorSettings settings) : base(reader, settings)
        {
        }

        public override bool IsNext(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            return position <= Reader.Length / Settings.PageSize;
        }

        protected override byte[] Read()
        {
            var length = LeftBytes < Settings.PageSize ? (int) LeftBytes : Settings.PageSize;
            LeftBytes -= length;
            return Reader.Read(length);
        }
    }
}
