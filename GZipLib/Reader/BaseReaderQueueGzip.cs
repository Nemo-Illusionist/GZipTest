using System;
using System.Collections.Generic;

namespace GZipLib.Reader
{
    public abstract class BaseReaderQueueGzip
    {
        protected IReader Reader { get; }

        protected BaseReaderQueueGzip(IReader reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        protected long ReadBlock(long leftBytes, List<byte> gzipBlock, byte[] header)
        {
            var headerCount = 0;
            while (leftBytes > 0)
            {
                var curByte = Reader.Read(Reader.Length - leftBytes);
                gzipBlock.Add(curByte);
                leftBytes--;

                if (curByte == header[headerCount])
                {
                    headerCount++;
                    if (headerCount != header.Length)
                    {
                        continue;
                    }
                    else
                    {
                        gzipBlock.RemoveRange(gzipBlock.Count - header.Length, header.Length);
                        break;
                    }
                }

                headerCount = 0;
            }

            return leftBytes;
        }

        protected byte[] ReadHeader()
        {
            return Reader.Read(0, Constants.DefaultHeader.Length);
        }
    }
}