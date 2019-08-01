using System;
using System.Collections.Generic;
using System.Threading;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public class WriterQueue : IWriterQueue
    {
        private readonly Dictionary<long, byte[]> _parts;
        private INextCheck _nextCheck;
        private readonly IWriter _writer;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly AutoResetEvent _waitHandler;
        private Thread _thread;

        private volatile Exception _exception;

        public WriterQueue(IWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));

            _parts = new Dictionary<long, byte[]>();
            _cancellationToken = new CancellationTokenSource();
            _waitHandler = new AutoResetEvent(false);
            _exception = null;
        }

        public void Add(long position, byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            lock (_parts)
            {
                _parts.Add(position, bytes);
                _waitHandler.Set();
            }
        }

        public void Start(INextCheck nextCheck)
        {
            if (_thread != null) return;
            _nextCheck = nextCheck ?? throw new ArgumentNullException(nameof(nextCheck));

            _thread = new Thread(WriterThread)
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public void Cancel()
        {
            _cancellationToken.Cancel();
            _waitHandler.Set();
        }

        public void Join()
        {
            _thread.Join();
            if (_exception != null) throw _exception;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _cancellationToken?.Dispose();
            _waitHandler?.Dispose();
        }

        private void WriterThread()
        {
            try
            {
                var token = _cancellationToken.Token;
                var position = 0;
                var isWait = true;

                while (_nextCheck.IsNext(position))
                {
                    if (isWait)
                    {
                        _waitHandler.WaitOne();
                    }

                    token.ThrowIfCancellationRequested();

                    byte[] bytes;
                    lock (_parts)
                    {
                        if (!_parts.TryGetValue(position, out bytes))
                        {
                            isWait = true;
                            continue;
                        }

                        isWait = false;
                        _parts.Remove(position);
                    }

                    _writer.Write(bytes);
                    position++;
                }
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