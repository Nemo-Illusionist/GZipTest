using System;
using System.Collections.Generic;

namespace GZipLib.Reader
{
    public class ReaderQueueGzipDecompress : IReaderQueue
    {
        private static readonly byte[] DefaultHeader = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00};
        private readonly IReadOnlyList<byte> _header;
        private readonly IReader _reader;
        private readonly int _bufferSize;

        private long _leftBytes;
        private int _index;

        public ReaderQueueGzipDecompress(IReader reader, int bufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferSize = bufferSize;
            _index = 0;
            _leftBytes = reader.Length;
            _header = ReadHeader();
        }


        public ReadingPart Next()
        {
            int index;
            var gzipBlock = new List<byte>(_bufferSize);
            gzipBlock.AddRange(_header);
            var gzipHeaderMatchsCount = 0;

            lock (_reader)
            {
                if (_leftBytes <= 0) return null;

                while (_leftBytes > 0)
                {
                    var curByte = _reader.Read(_reader.Length - _leftBytes);
                    gzipBlock.Add(curByte);
                    _leftBytes--;

                    if (curByte == _header[gzipHeaderMatchsCount])
                    {
                        gzipHeaderMatchsCount++;
                        if (gzipHeaderMatchsCount != _header.Count)
                            continue;

                        gzipBlock.RemoveRange(gzipBlock.Count - _header.Count, _header.Count);
                        break;
                    }

                    gzipHeaderMatchsCount = 0;
                }

                index = _index++;
            }

            return new ReadingPart(index, gzipBlock.ToArray());
        }

        private byte[] ReadHeader()
        {
            return _reader.Read(0, DefaultHeader.Length);
        }

        public bool IsNext(long position)
        {
            return !(_leftBytes <= 0 && _index == position);
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
