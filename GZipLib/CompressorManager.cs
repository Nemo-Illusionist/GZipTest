using System;
using System.IO.Compression;
using System.Threading;
using GZipLib.Compressor;
using GZipLib.Queue;
using GZipLib.Reader;
using GZipLib.Settings;
using GZipLib.Writer;

namespace GZipLib
{
    public class CompressorManager : IDisposable
    {
        private readonly IWriterJobFactory _writerJobFactory;
        private readonly IReaderJobFactory _readerJobFactory;
        private readonly ICompressor _compressor;
        private readonly CompressorSettings _settings;
        private readonly IQueue _readerQueue;
        private readonly IQueue _writerQueue;
        private readonly CancellationTokenSource _cancellationToken;

        private IReaderJob _readerJob;
        private IWriterJob _writerJob;

        private volatile Exception _exception;


        public CompressorManager(IQueue readerQueue, IQueue writerQueue,
            IWriterJobFactory writerJobFactory, IReaderJobFactory readerJobFactory,
            ICompressor compressor, CompressorSettings settings)
            : this(writerJobFactory, readerJobFactory, compressor, settings)
        {
            _readerQueue = readerQueue ?? throw new ArgumentNullException(nameof(readerQueue));
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
        }

        public CompressorManager(IWriterJobFactory writerJobFactory, IReaderJobFactory readerJobFactory,
            ICompressor compressor, CompressorSettings settings) : this()
        {
            _writerJobFactory = writerJobFactory ?? throw new ArgumentNullException(nameof(writerJobFactory));
            _readerJobFactory = readerJobFactory ?? throw new ArgumentNullException(nameof(readerJobFactory));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _readerQueue = new Queue.Queue();
            _writerQueue = new Queue.Queue();
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public void Run(CompressionMode mode)
        {
            var token = _cancellationToken.Token;

            var method = CompressorMode(mode);

            _readerJob = _readerJobFactory.Create(_readerQueue, mode);
            _writerJob = _writerJobFactory.Create(_writerQueue, mode);

            _readerJob.Start();
            _writerJob.Start(_readerJob);

            for (int i = 0; i < _settings.ThreadPoolSize; i++)
            {
                var thread = new Thread(() => { CompressorThread(token, method); });
                thread.Start();
            }
        }

        private void CompressorThread(CancellationToken token, Func<byte[], byte[]> method)
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
        }

        public void Join()
        {
            _readerJob.Join();
            _writerJob.Join();
            if (_exception != null) throw _exception;
        }

        public void Cancel()
        {
            _readerJob.Cancel();
            _writerJob.Cancel();
            _cancellationToken.Cancel();
        }

        public void Dispose()
        {
            _readerJob?.Dispose();
            _readerQueue?.Dispose();

            _writerJob?.Dispose();
            _writerQueue?.Dispose();

            _cancellationToken?.Dispose();
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