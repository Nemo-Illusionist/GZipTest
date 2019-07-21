using System;
using System.Collections.Generic;

namespace GZipLib.Reader
{
    public class ReaderQueueGzipWithCache : BaseReaderQueueGzip, IReaderQueue
    {
        private readonly Dictionary<int, (long, int)> _dictionary;
        private readonly IReader _reader;
        private int _index;

        public ReaderQueueGzipWithCache(IReader reader, IReader blockReader, int bufferSize) : base(blockReader)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _dictionary = new Dictionary<int, (long, int)>();
            Start(blockReader.Length, bufferSize);
            _index = 0;
        }

        public ReadingPart Next()
        {
            (long, int) positionAndLength;
            int index;
            lock (_reader)
            {
                index = _index;
                if (!_dictionary.TryGetValue(index, out positionAndLength)) return null;
                positionAndLength = _dictionary[index];
                _dictionary.Remove(index);
                _index++;
            }

            return new ReadingPart(index, _reader.Read(positionAndLength.Item1, positionAndLength.Item2));
        }

        public bool IsNext(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            return !(_dictionary.Count == 0 && _index == position);
        }

        public void Dispose()
        {
            Reader?.Dispose();
            _reader?.Dispose();
        }

        private void Start(long fileLength, int bufferSize)
        {
            byte[] header = ReadHeader();
            var leftBytes = fileLength - header.Length;
            var chunkIndex = 0;
            while (leftBytes > 0)
            {
                var gzipBlock = new List<byte>(bufferSize);
                gzipBlock.AddRange(header);

                leftBytes = ReadBlock(leftBytes, gzipBlock, header);

                var length = gzipBlock.ToArray().Length;
                var position = fileLength - leftBytes - header.Length - length;
                if (position + header.Length + length == fileLength)
                {
                    position += header.Length;
                }

                _dictionary.Add(chunkIndex, (position, length));

                chunkIndex++;
            }
        }
    }
}