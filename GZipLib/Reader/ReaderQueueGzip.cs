using System;
using System.Collections.Generic;
using System.Linq;

namespace GZipLib.Reader
{
    public class ReaderQueueGzip : BaseReaderQueueGzip, IReaderQueue
    {
        private readonly IReadOnlyList<byte> _header;
        private readonly int _bufferSize;

        private long _leftBytes;
        private int _index;

        public ReaderQueueGzip(IReader reader, int bufferSize) : base(reader)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            _bufferSize = bufferSize;
            _header = ReadHeader();
            _leftBytes = reader.Length - _header.Count;
            _index = 0;
        }

        public ReadingPart Next()
        {
            int index;
            var gzipBlock = new List<byte>(_bufferSize);
            gzipBlock.AddRange(_header);

            lock (Reader)
            {
                if (_leftBytes <= 0) return null;

                _leftBytes = ReadBlock(_leftBytes, gzipBlock, _header.ToArray());
                index = _index++;
            }

            return new ReadingPart(index, gzipBlock.ToArray());
        }

        public bool IsNext(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            return !(_leftBytes <= 0 && _index == position);
        }

        public void Dispose()
        {
            Reader?.Dispose();
        }
    }
}