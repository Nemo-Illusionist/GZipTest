using System;
using System.IO;
using System.IO.Compression;

namespace GZipLib.Compressor
{
    public class GZipCompressor : ICompressor
    {
        private readonly int _bufferSize;

        public GZipCompressor(int bufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            _bufferSize = bufferSize;
        }

        public byte[] Compress(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using (var output = new MemoryStream())
            {
                using (var compress = new GZipStream(output, CompressionMode.Compress))
                {
                    compress.Write(bytes, 0, bytes.Length);
                }

                return output.ToArray();
            }
        }

        public byte[] Decompress(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(bytes))
                {
                    using (var decompress = new GZipStream(input, CompressionMode.Decompress))
                    {
                        Copy(decompress, output);
                    }
                }

                return output.ToArray();
            }
        }

        private void Copy(Stream input, Stream output)
        {
            var buffer = new byte[_bufferSize];
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}