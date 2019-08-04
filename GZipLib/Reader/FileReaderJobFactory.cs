using System;
using System.IO.Compression;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public class FileReaderJobFactory : IReaderJobFactory
    {
        private readonly string _filePath;
        private readonly CompressorSettings _settings;

        public FileReaderJobFactory(string filePath, CompressorSettings settings)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(filePath));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _filePath = filePath;
        }

        public IReaderJob Create(IReaderQueue queue, CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                    return new ReaderJob(new FileReader(_filePath), queue, _settings);
                case CompressionMode.Decompress:
                    return new ReaderJobGzip(new FileReader(_filePath), queue, _settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }
        }
    }
}