using System;
using System.IO.Compression;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public class FileWriterJobFactory : IWriterJobFactory
    {
        private readonly string _filePath;

        public FileWriterJobFactory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));
            _filePath = filePath;
        }

        public IWriterJob Create(IWriterQueue queue, CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                case CompressionMode.Decompress:
                    return new WriterJob(new FileWriter(_filePath), queue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }
        }
    }
}