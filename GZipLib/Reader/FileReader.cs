using System;
using System.IO;

namespace GZipLib.Reader
{
    public class FileReader : IReader
    {
        public long Length { get; }

        private readonly string _filePath;

        public FileReader(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));

            _filePath = filePath;
            Length = new FileInfo(_filePath).Length;
        }


        public byte[] Read(long position, int length)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var bytes = new byte[length];
            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(position, SeekOrigin.Begin);
                stream.Read(bytes, 0, length);
            }

            return bytes;
        }

        public byte Read(long position)
        {
            if (position <= 0) throw new ArgumentOutOfRangeException(nameof(position));
            byte readByte;
            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(position, SeekOrigin.Begin);
                readByte = (byte) stream.ReadByte();
            }

            return readByte;
        }

        public void Dispose()
        {
        }
    }
}
