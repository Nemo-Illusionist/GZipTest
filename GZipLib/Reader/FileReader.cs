using System;
using System.IO;

namespace GZipLib.Reader
{
    public class FileReader : IReader
    {
        public long Length { get; }
        private readonly FileStream _stream;

        public FileReader(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));

            Length = new FileInfo(filePath).Length;
            _stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public byte[] Read(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var bytes = new byte[length];

            lock (_stream)
            {
                _stream.Read(bytes, 0, length);
            }

            return bytes;
        }

        public byte Read()
        {
            lock (_stream)
            {
                return (byte) _stream.ReadByte();
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
