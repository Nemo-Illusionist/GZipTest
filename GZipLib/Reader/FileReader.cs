using System;
using System.IO;

namespace GZipLib.Reader
{
    public class FileReader : IReader
    {
        public long LeftBytes { get; private set; }
        private readonly FileStream _stream;

        public FileReader(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));

            LeftBytes = new FileInfo(filePath).Length;
            _stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public byte[] Read(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            var bytes = new byte[length];

            lock (_stream)
            {
                _stream.Read(bytes, 0, length);
                LeftBytes -= length;
            }

            return bytes;
        }

        public byte Read()
        {
            byte b;
            lock (_stream)
            {
                b = (byte) _stream.ReadByte();
                LeftBytes--;
            }

            return b;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}