using System;
using System.IO;

namespace GZipLib.Writer
{
    public sealed class FileWriter : IWriter
    {
        private  FileStream _stream;

        public FileWriter(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));

            _stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
        }

        public void Write(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _stream.Seek(0, SeekOrigin.End);
            _stream.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}