using System;
using System.Collections.Generic;
using GZipLib.Reader;

namespace GZipLib.Queue
{
    public class Queue : IQueue
    {
        public event EventHandler AddEvent;
        public event EventHandler NextEvent;

        private readonly Dictionary<long, byte[]> _parts;
        private int _index;

        public Queue()
        {
            _parts = new Dictionary<long, byte[]>();
            _index = 0;
        }

        public int Count()
        {
            lock (_parts)
            {
                return _parts.Count;
            }
        }

        public void Add(long position, byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            lock (_parts)
            {
                _parts.Add(position, bytes);
                AddEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public ReadingPart Next()
        {
            ReadingPart part = null;

            lock (_parts)
            {
                if (_parts.TryGetValue(_index, out var bytes))
                {
                    _parts.Remove(_index);
                    part = new ReadingPart(_index++, bytes);
                    NextEvent?.Invoke(this, EventArgs.Empty);
                }
            }

            return part;
        }

        public void Dispose()
        {
        }
    }
}