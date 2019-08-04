using System;

namespace GZipLib.Core
{
    public class Part
    {
        public int Index { get; }
        public byte[] Data { get; }

        public Part(int index, byte[] data)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Index = index;
        }
    }
}