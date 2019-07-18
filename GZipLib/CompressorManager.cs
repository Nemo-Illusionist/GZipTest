using System;
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
        private readonly IReaderQueue _readerQueue;
        private readonly ICompressor _compressor;
        private readonly CancellationTokenSource _cancellationToken;

        private readonly int _threadPoolSize;

        public CompressorManager(IWriterQueue writerQueue, IReaderQueue readerQueue,
            ICompressor compressor, int threadPoolSize) : this()
        {
            if (threadPoolSize <= 0) throw new ArgumentOutOfRangeException(nameof(threadPoolSize));
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
            _readerQueue = readerQueue ?? throw new ArgumentNullException(nameof(readerQueue));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _threadPoolSize = threadPoolSize;
        }

        public CompressorManager(string input, string output) : this()
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Value cannot be null or empty.", nameof(input));
            if (string.IsNullOrEmpty(output))
                throw new ArgumentException("Value cannot be null or empty.", nameof(output));

            var settings = new CompressorSettings();
            var reader = new FileReader(input);
            _readerQueue = new ReaderQueue(reader, settings.PageSize);
            _writerQueue = new WriterQueue(_readerQueue, new FileWriter(output));
            _compressor = new GZipCompressor(settings.BufferSize);
            _threadPoolSize = settings.ThreadPoolSize;
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public void Compress()
        {
            var token = _cancellationToken.Token;

            _writerQueue.Start();
            for (int i = 0; i < _threadPoolSize; i++)
            {
                var thread = new Thread(() =>
                {
                    var part = _readerQueue.Next();
                    while (part != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var bytes = part.Data;
                        bytes = _compressor.Compress(bytes);
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
            _writerQueue?.Dispose();
        }
    }
}