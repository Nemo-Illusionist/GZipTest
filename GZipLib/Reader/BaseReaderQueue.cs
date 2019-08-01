using System;
using System.Collections.Generic;
using System.Threading;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public abstract class BaseReaderQueue : IReaderQueue
    {
        protected IReader Reader { get; }
        protected CompressorSettings Settings { get; }

        private readonly Dictionary<long, byte[]> _parts;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly AutoResetEvent _readerWaitHandler;
        private readonly AutoResetEvent _nextWaitHandler;

        private Thread _thread;
        private int _count;
        private int _index;
        private bool _isCancel;

        private volatile Exception _exception;

        protected BaseReaderQueue(IReader reader, CompressorSettings settings)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _index = 0;
            _count = 0;
            _isCancel = false;
            _parts = new Dictionary<long, byte[]>();
            _cancellationToken = new CancellationTokenSource();
            _readerWaitHandler = new AutoResetEvent(false);
            _nextWaitHandler = new AutoResetEvent(false);
            _exception = null;
        }

        public void Start()
        {
            if (_thread != null) return;

            _thread = new Thread(ReaderThread)
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public ReadingPart Next()
        {
            ReadingPart part = null;
            var isWait = false;
            while (part == null)
            {
                if (isWait)
                {
                    _nextWaitHandler.WaitOne();
                }

                lock (_parts)
                {
                    if (!IsNext(_index)) return null;

                    if (!_parts.TryGetValue(_index, out var bytes))
                    {
                        isWait = true;
                    }
                    else
                    {
                        _parts.Remove(_index);
                        part = new ReadingPart(_index++, bytes);
                    }
                }

                _readerWaitHandler.Set();
            }

            return part;
        }

        public bool IsNext(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            return !_isCancel && !(Reader.LeftBytes <= 0 && position == _count);
        }

        public void Cancel()
        {
            _isCancel = true;
            _cancellationToken.Cancel();
            _readerWaitHandler.Set();
            _nextWaitHandler.Set();
        }

        public void Join()
        {
            _thread.Join();
            if (_exception != null) throw _exception;
        }

        public void Dispose()
        {
            _readerWaitHandler.Dispose();
            _nextWaitHandler.Dispose();
            _cancellationToken.Dispose();
            Reader.Dispose();
        }

        protected abstract byte[] Read();

        private void ReaderThread()
        {
            try
            {
                var token = _cancellationToken.Token;

                var isWait = false;
                var index = 0;

                while (Reader.LeftBytes > 0)
                {
                    if (isWait)
                    {
                        _readerWaitHandler.WaitOne();
                    }

                    token.ThrowIfCancellationRequested();

                    lock (_parts)
                    {
                        if (_parts.Count >= Settings.ThreadPoolSize)
                        {
                            isWait = true;
                            continue;
                        }

                        isWait = false;
                    }

                    var bytes = Read();

                    lock (_parts)
                    {
                        _parts.Add(index, bytes);
                        _nextWaitHandler.Set();
                    }

                    index++;
                }

                _count = index;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }
    }
}