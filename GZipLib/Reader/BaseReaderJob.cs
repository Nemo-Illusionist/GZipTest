using System;
using System.Threading;
using GZipLib.Job;
using GZipLib.Settings;

namespace GZipLib.Reader
{
    public abstract class BaseReaderJob : BaseJob, IReaderJob
    {
        protected IReader Reader { get; }
        protected CompressorSettings Settings { get; }

        private readonly IReaderQueue _queue;

        private volatile int _count;

        protected BaseReaderJob(IReader reader, IReaderQueue queue, CompressorSettings settings)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _queue.NextEvent += WaitHandlerSet;
            _count = 0;
        }

        public bool IsNext(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            return !(Reader.LeftBytes <= 0 && position == _count);
        }

        public override void Dispose()
        {
            base.Dispose();
            Reader.Dispose();
        }

        protected abstract byte[] Read();

        protected override void JobThread(CancellationToken token)
        {
            var index = 0;

            while (Reader.LeftBytes > 0)
            {
                WaitHandler.WaitOne();
                token.ThrowIfCancellationRequested();

                if (_queue.Count() >= Settings.ThreadPoolSize)
                {
                    WaitHandler.Reset();
                    continue;
                }

                WaitHandler.Set();

                var bytes = Read();
                _queue.Add(index, bytes);

                index++;
            }

            _count = index;
        }
    }
}