using System;
using System.IO;

namespace GZipLib.Reader
{
    public class FileReader : IReader
    {
        private readonly string _filePath;

        public FileReader(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));

            _filePath = filePath;
        }

        public byte[] Read(int position, int length)
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
    }
}