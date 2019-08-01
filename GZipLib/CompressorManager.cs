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

        private volatile Exception _exception;

        public CompressorManager(IWriterQueue writerQueue, IReaderQueueFactory readerQueueFactory,
            ICompressor compressor, CompressorSettings settings) : this()
        {
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
            _readerQueueFactory = readerQueueFactory ?? throw new ArgumentNullException(nameof(readerQueueFactory));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _exception = null;
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public void Run(CompressionMode mode)
        {
            var token = _cancellationToken.Token;

            var method = CompressorMode(mode);
            _readerQueue = _readerQueueFactory.Create(mode);
            _readerQueue.Start();
            _writerQueue.Start(_readerQueue);
            for (int i = 0; i < _settings.ThreadPoolSize; i++)
            {
                var thread = new Thread(() =>
                {
                    try
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
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception e)
                    {
                        _exception = e;
                        Cancel();
                    }
                });
                thread.Start();
            }
        }

        public void Join()
        {
            _readerQueue.Join();
            _writerQueue.Join();
            if (_exception != null) throw _exception;
        }

        public void Cancel()
        {
            _cancellationToken.Cancel();
            _writerQueue.Cancel();
            _readerQueue.Cancel();
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