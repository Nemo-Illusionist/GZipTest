using System;
using System.IO.Compression;
using System.Threading;
using GZipLib.Compressor;
using GZipLib.Reader;
using GZipLib.Settings;
using GZipLib.Writer;

namespace GZipLib
{
    public class CompressorManager : IDisposable
    {
        private readonly IWriterQueue _writerQueue;
        private readonly IReaderQueueFactory _readerQueueFactory;
        private readonly ICompressor _compressor;
        private readonly CompressorSettings _settings;
        private readonly CancellationTokenSource _cancellationToken;

        private IReaderQueue _readerQueue;

        public CompressorManager(IWriterQueue writerQueue, IReaderQueueFactory readerQueueFactory,
            ICompressor compressor, CompressorSettings settings) : this()
        {
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
            _readerQueueFactory = readerQueueFactory ?? throw new ArgumentNullException(nameof(readerQueueFactory));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public void Run(CompressionMode mode)
        {
            var method = CompressorMode(mode);

            var token = _cancellationToken.Token;

            _readerQueue = _readerQueueFactory.Create(mode);
            _writerQueue.Start(_readerQueue);
            for (int i = 0; i < _settings.ThreadPoolSize; i++)
            {
                var thread = new Thread(() =>
                {
                    var part = _readerQueue.Next();
                    while (part != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var bytes = part.Data;
                        bytes = method(bytes);
                        _writerQueue.Add(part.Index, bytes);
                        part = _readerQueue.Next();
                    }
                });
                thread.Start();
            }
        }

        public void Join()
        {
            _writerQueue.Join();
        }

        public void Cancel()
        {
            _writerQueue.Cancel();
            _cancellationToken.Cancel();
        }

        public void Dispose()
        {
            _cancellationToken?.Dispose();
            _readerQueue?.Dispose();
            _writerQueue?.Dispose();
        }

        private Func<byte[], byte[]> CompressorMode(CompressionMode mode)
        {
            Func<byte[], byte[]> method;
            switch (mode)
            {
                case CompressionMode.Compress:
                    method = _compressor.Compress;
                    break;
                case CompressionMode.Decompress:
                    method = _compressor.Decompress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mod is not supported.");
            }

            return method;
        }
    }
}
