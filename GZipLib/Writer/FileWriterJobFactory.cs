using System;
using System.IO.Compression;
using GZipLib.Core;
using GZipLib.Job;

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

        public IJob Create(IWriterQueue queue, INextCheck nextCheck, CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                case CompressionMode.Decompress:
                    return new WriterJob(new FileWriter(_filePath), nextCheck, queue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }
        }
    }
}