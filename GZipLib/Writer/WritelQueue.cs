using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GZipLib.Writer
{
    public class WritelQueue : IWritelQueue
    {
        private readonly Dictionary<long, byte[]> _parts;
        private readonly long _count;
        private readonly IWriter _writer;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly AutoResetEvent _waitHandler;
        private Thread _thread;

        private WritelQueue(long count, IWriter writer)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            _count = count;

            _writer = writer ?? throw new ArgumentNullException(nameof(writer));

            _parts = new Dictionary<long, byte[]>();
            _cancellationToken = new CancellationTokenSource();
            _waitHandler = new AutoResetEvent(false);
        }

        public static WritelQueue CreateAndStart(long count, IWriter writer)
        {
            var writelQueue = new WritelQueue(count, writer);

            writelQueue._thread = new Thread(writelQueue.Start)
            {
                IsBackground = true
            };
            writelQueue._thread.Start();

            return writelQueue;
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

        public void Cancel()
        {
            _cancellationToken.Cancel();
            _waitHandler.Set();
        }

        public void Join()
        {
            _thread.Join();
        }

        private void Start()
        {
            var token = _cancellationToken.Token;
            var position = 0;
            var isWait = true;

            while (position <= _count)
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
    }
}