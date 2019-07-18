using System;
using System.IO;

namespace GZipLib.Reader
{
    public class FileReaderWithOpenStream : IReader
    {
        public long Length { get; }
        private readonly FileStream _stream;

        public FileReaderWithOpenStream(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));

            Length = new FileInfo(filePath).Length;
            _stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public byte[] Read(long position, int length)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var bytes = new byte[length];
            _stream.Seek(position, SeekOrigin.Begin);
            _stream.Read(bytes, 0, length);

            return bytes;
        }

        public byte Read(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            _stream.Seek(position, SeekOrigin.Begin);
            return (byte) _stream.ReadByte();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
