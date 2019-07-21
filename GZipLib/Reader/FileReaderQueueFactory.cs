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
                    return new ReaderQueue(new FileReader(_filePath), _settings.PageSize);
                case CompressionMode.Decompress when !_settings.ReaderQueueWithCache:
                    return new ReaderQueueGzip(new FileReaderWithOpenStream(_filePath), _settings.PageSize);
                case CompressionMode.Decompress when _settings.ReaderQueueWithCache:
                    return new ReaderQueueGzipWithCache(new FileReader(_filePath),
                        new FileReaderWithOpenStream(_filePath), _settings.PageSize);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }
        }
    }
}