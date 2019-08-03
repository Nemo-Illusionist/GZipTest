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
            var isWait = true;

            while (_nextCheck.IsNext(position))
            {
                if (isWait)
                {
                    WaitHandler.WaitOne();
                }

                token.ThrowIfCancellationRequested();

                byte[] bytes;
                lock (_queue)
                {
                    var readingPart = _queue.Next();
                    if (readingPart == null)
                    {
                        isWait = true;
                        continue;
                    }

                    bytes = readingPart.Data;
                    isWait = false;
                }

                _writer.Write(bytes);
                position++;
            }
        }
    }
}