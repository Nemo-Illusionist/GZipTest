using System;
using System.Threading;
using GZipLib.Core;
using GZipLib.Job;
using GZipLib.Queue;

namespace GZipLib.Writer
{
    public class WriterJob : BaseJob, IWriterJob
    {
        private readonly IWriter _writer;
        private readonly IWriterQueue _queue;

        private INextCheck _nextCheck;

        public WriterJob(IWriter writer, IWriterQueue queue)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _queue.AddEvent += WaitHandlerSet;
        }

        public void Start(INextCheck nextCheck)
        {
            if (CeckStart()) return;

            _nextCheck = nextCheck ?? throw new ArgumentNullException(nameof(nextCheck));
            StartThread();
        }

        public override void Dispose()
        {
            base.Dispose();
            _writer?.Dispose();
        }

        protected override void JobThread(CancellationToken token)
        {
            var position = 0;

            while (_nextCheck.IsNext(position))
            {
                WaitHandler.WaitOne();
                token.ThrowIfCancellationRequested();

                var readingPart = _queue.Next();
                if (readingPart == null)
                {
                    WaitHandler.Reset();
                    continue;
                }

                WaitHandler.Set();

                var bytes = readingPart.Data;
                _writer.Write(bytes);

                position++;
            }
        }
    }
}