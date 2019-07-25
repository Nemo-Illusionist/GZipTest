using System;
using System.Collections.Generic;
using System.Threading;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public abstract class BaseReaderQueue : IReaderQueue
    {
        public event EventHandler EndQueueEvent;

        protected IReader Reader { get; }
        protected int Index { get; private set; }
        protected long LeftBytes { get; set; }
        protected CompressorSettings Settings { get; }


        private readonly Dictionary<long, byte[]> _parts;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly AutoResetEvent _readerWaitHandler;

        private Thread _thread;


        protected BaseReaderQueue(IReader reader, CompressorSettings settings)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            Index = 0;
            LeftBytes = Reader.Length;
            _parts = new Dictionary<long, byte[]>();
            _cancellationToken = new CancellationTokenSource();
            _readerWaitHandler = new AutoResetEvent(false);
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
            ReadingPart part;
            lock (_parts)
            {
                if (!IsNext(Index)) return null;

                //todo: need to add waiting
                if (!_parts.TryGetValue(Index, out var bytes)) return null;

                _parts.Remove(Index);
                _readerWaitHandler.Set();
                part = new ReadingPart(Index++, bytes);
            }

            return part;
        }

        public abstract bool IsNext(long position);

        public void Cancel()
        {
            _cancellationToken.Cancel();
            _readerWaitHandler.Set();
        }

        public void Join()
        {
            _thread.Join();
        }

        public void Dispose()
        {
            _readerWaitHandler.Dispose();
            _cancellationToken.Dispose();
            Reader.Dispose();
        }

        protected abstract byte[] Read();

        private void ReaderThread()
        {
            var token = _cancellationToken.Token;

            var isWait = false;
            var index = 0;

            while (LeftBytes > 0)
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
                }

                index++;
            }

            EndQueueEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
