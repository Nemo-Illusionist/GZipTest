using System;
using System.Threading;
using GZipLib.Compressor;
using GZipLib.Reader;
using GZipLib.Settings;
using GZipLib.Writer;

namespace GZipLib
{
    public class CompressorManager
    {
        private readonly IWriterQueue _writerQueue;
        private readonly IReaderQueue _readerQueue;
        private readonly IReader _reader;
        private readonly ICompressor _compressor;
        private readonly CompressorSettings _settings;
        private readonly CancellationTokenSource _cancellationToken;


        public CompressorManager(IWriterQueue writerQueue, IReaderQueue readerQueue, IReader reader,
            ICompressor compressor, CompressorSettings settings) : this()
        {
            _writerQueue = writerQueue ?? throw new ArgumentNullException(nameof(writerQueue));
            _readerQueue = readerQueue ?? throw new ArgumentNullException(nameof(readerQueue));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public CompressorManager(string input, string output) : this()
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Value cannot be null or empty.", nameof(input));
            if (string.IsNullOrEmpty(output))
                throw new ArgumentException("Value cannot be null or empty.", nameof(output));
            
            _settings = new CompressorSettings();
            _reader = new FileReader(input);
            _writerQueue = new WriterQueue(_reader.Length / _settings.PageSize, new FileWriter(output));
            _readerQueue = new ReaderQueue(_reader, _settings.PageSize);
            _compressor = new GZipCompressor(_settings.BufferSize);
        }

        private CompressorManager()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public void Compress()
        {
            var token = _cancellationToken.Token;
            for (int i = 0; i < _settings.ThreadPoolSize; i++)
            {
                var thread = new Thread(() =>
                {
                    var next = _readerQueue.Next();
                    while (next.HasValue)
                    {
                        token.ThrowIfCancellationRequested();

                        var bytes = _reader.Read(next.Value.position, next.Value.length);
                        bytes = _compressor.Compress(bytes);
                        _writerQueue.Add(next.Value.index, bytes);
                        next = _readerQueue.Next();
                    }
                });
                thread.Start();
            }

            _writerQueue.Start();
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
    }
}