using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using GZipLib.Compressor;
using GZipLib.Job;
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
        private readonly List<AutoResetEvent> _waitHandlers;

        private volatile IReaderJob _readerJob;
        private volatile IJob _writerJob;
        private volatile Exception _exception;


        public CompressorManager(IQueue readerQueue, IQueue writerQueue,
            IWriterJobFactory writerJobFactory, IReaderJobFactory readerJobFactory,
            ICompressor compressor, CompressorSettings settings)
            : this()
        {
            _writerJobFactory = writerJobFactory ?? throw new ArgumentNullException(nameof(writerJobFactory));
            _readerJobFactory = readerJobFactory ?? throw new ArgumentNullException(nameof(readerJobFactory));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _readerQueue = readerQueue ?? throw new ArgumentNullException(nameof(readerQueue));
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
            _waitHandlers = new List<AutoResetEvent>();
        }

        public void Run(CompressionMode mode)
        {
            var cancellationToken = _cancellationToken.Token;
            var method = CompressorMode(mode);

            _readerJob = _readerJobFactory.Create(_readerQueue, mode);
            _writerJob = _writerJobFactory.Create(_writerQueue, _readerJob, mode);


            _readerJob.Start();
            _writerJob.Start();

            for (int i = 0; i < _settings.ThreadPoolSize; i++)
            {
                var waitHandler = new AutoResetEvent(true);
                var thread = new Thread(() => { CompressorThread(method, waitHandler, cancellationToken); });
                thread.Start();
                _readerQueue.AddEvent += (e, s) => waitHandler.Set();
                _waitHandlers.Add(waitHandler);
            }
        }

        private void CompressorThread(Func<byte[], byte[]> method,
            AutoResetEvent waitHandler, CancellationToken cancellationToken)
        {
            try
            {
                var part = _readerQueue.Next();
                while (_readerJob.IsAlive() || part != null)
                {
                    if (_readerJob.IsAlive() && part == null)
                    {
                        waitHandler.Reset();
                    }

                    waitHandler.WaitOne();
                    cancellationToken.ThrowIfCancellationRequested();

                    if (part != null)
                    {
                        var bytes = part.Data;
                        bytes = method(bytes);
                        _writerQueue.Add(part.Index, bytes);
                    }

                    part = _readerQueue.Next();
                    waitHandler.Set();
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
            try
            {
                _readerJob.Join();
                _writerJob.Join();
            }
            catch (Exception)
            {
                Cancel();
                throw;
            }

            if (_exception != null) throw _exception;
        }

        public void Cancel()
        {
            _readerJob.Cancel();
            _writerJob.Cancel();
            _cancellationToken.Cancel();
            foreach (var waitHandler in _waitHandlers)
            {
                waitHandler.Set();
            }
        }

        public void Dispose()
        {
            _readerJob?.Dispose();
            _writerJob?.Dispose();

            _cancellationToken?.Dispose();
            foreach (var waitHandler in _waitHandlers)
            {
                waitHandler.Dispose();
            }
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