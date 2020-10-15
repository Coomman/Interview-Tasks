using System;
using System.Threading;

namespace GZipTest.Core
{
    public class MaximumCountEvent
    {
        private readonly object _locker = new object();

        public int MaximumCount { get; }
        private int _currentCount;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        public MaximumCountEvent(int maximumCount)
        {
            if (maximumCount < 1)
                throw new ArgumentException("Maximum count must be at least 1");

            MaximumCount = maximumCount;
        }

        public void Add()
        {
            _resetEvent.WaitOne();

            lock (_locker)
            {
                _currentCount++;

                if (_currentCount != MaximumCount)
                    _resetEvent.Set();
            }
        }
        public void Signal()
        {
            lock (_locker)
            {
                if (_currentCount == 0)
                    throw new InvalidOperationException("Current count was already 0");

                _currentCount--;

                _resetEvent.Set();
            }
        }
    }
}
