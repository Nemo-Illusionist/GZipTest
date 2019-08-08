using System;
using System.Threading;
using GZipLib.Core;
using GZipLib.Job;

namespace GZipLib.Writer
{
    public class WriterJob : BaseJob
    {
        private readonly IWriter _writer;
        private readonly IWriterQueue _queue;
        private readonly INextCheck _nextCheck;

        public WriterJob(IWriter writer, INextCheck nextCheck, IWriterQueue queue)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _nextCheck = nextCheck ?? throw new ArgumentNullException(nameof(nextCheck));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _queue.AddEvent += WaitHandlerSet;
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
                token.ThrowIfCancellationRequested();

                var readingPart = _queue.Next();
                if (readingPart == null)
                {
                    WaitHandler.WaitOne();
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