using System;
using System.IO.Compression;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class FileReaderQueueFactory : IReaderQueueFactory
    {
        private readonly string _filePath;
        private readonly CompressorSettings _settings;

        public FileReaderQueueFactory(string filePath, CompressorSettings settings)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _filePath = filePath;
        }

        public IReaderQueue Create(CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                    return new ReaderQueue(new FileReader(_filePath), _settings);
                case CompressionMode.Decompress:
                    return new ReaderQueueGzip(new FileReader(_filePath), _settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }
        }
    }
}
