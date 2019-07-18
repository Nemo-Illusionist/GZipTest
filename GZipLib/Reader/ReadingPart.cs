using System;

namespace GZipLib.Reader
{
    public class ReadingPart
    {
        public int Index { get; }
        public byte[] Data { get; }

        public ReadingPart(int index, byte[] data)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Index = index;
        }
    }
}