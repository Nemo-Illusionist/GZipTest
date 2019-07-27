using System.Collections.Generic;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class ReaderQueueGzip : BaseReaderQueue
    {
        private readonly byte[] _header;

        public ReaderQueueGzip(IReader reader, CompressorSettings settings) : base(reader, settings)
        {
            _header = ReadHeader();
        }

        protected override byte[] Read()
        {
            var headerCount = 0;

            var bytes = new List<byte>(_header);
            while (Reader.LeftBytes > 0)
            {
                var curByte = Reader.Read();
                bytes.Add(curByte);

                if (curByte == _header[headerCount])
                {
                    headerCount++;
                    if (headerCount != _header.Length)
                    {
                        continue;
                    }
                    else
                    {
                        bytes.RemoveRange(bytes.Count - _header.Length, _header.Length);
                        break;
                    }
                }

                headerCount = 0;
            }

            return bytes.ToArray();
        }

        private byte[] ReadHeader()
        {
            return Reader.Read(Constants.DefaultGzipHeaderLength);
        }
    }
}