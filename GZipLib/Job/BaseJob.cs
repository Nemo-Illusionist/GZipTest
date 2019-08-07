using System;
using System.Threading;

namespace GZipLib.Job
{
    public abstract class BaseJob : IJob
    {
        protected AutoResetEvent WaitHandler { get; }

        private readonly CancellationTokenSource _cancellationToken;

        private Thread _thread;

        private volatile Exception _exception;

        protected BaseJob()
        {
            _cancellationToken = new CancellationTokenSource();
            WaitHandler = new AutoResetEvent(true);

            _exception = null;
            _thread = null;
        }

        public bool IsAlive()
        {
            return _thread.IsAlive;
        }

        public virtual void Cancel()
        {
            _cancellationToken.Cancel();
            WaitHandler.Set();
        }

        public virtual void Join()
        {
            _thread.Join();
            if (_exception != null) throw _exception;
        }

        public virtual void Dispose()
        {
            WaitHandler.Dispose();
            _cancellationToken.Dispose();
        }

        public void Start()
        {
            if (CeckStart()) return;

            _thread = new Thread(JobThread)
            {
                IsBackground = true
            };
            _thread.Start();
        }

        protected abstract void JobThread(CancellationToken token);

        protected void WaitHandlerSet(object sender, EventArgs e)
        {
            WaitHandler.Set();
        }

        protected bool CeckStart()
        {
            return _thread != null;
        }

        private void JobThread()
        {
            try
            {
                var token = _cancellationToken.Token;
                JobThread(token);
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