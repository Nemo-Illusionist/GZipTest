using System;

namespace GZipLib.Reader
{
    public class ReaderQueue : IReaderQueue
    {
        private readonly IReader _reader;
        private readonly int _pageSize;
        private long _position;
        private long _leftBytes;
        private int _index;

        public ReaderQueue(IReader reader, int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _pageSize = pageSize;
            _position = 0;
            _index = 0;
            _leftBytes = reader.Length;
        }

        public ReadingPart Next()
        {
            long position;
            int length;
            int index;
            lock (_reader)
            {
                if (_leftBytes <= 0) return null;

                _position = _reader.Length - _leftBytes;
                position = _position;
                length = _leftBytes < _pageSize ? (int) _leftBytes : _pageSize;
                _leftBytes -= length;
                index = _index++;
            }

            return new ReadingPart(index, _reader.Read(position, length));
        }

        public bool More(long position)
        {
            return position <= _reader.Length / _pageSize;
        }
    }
}