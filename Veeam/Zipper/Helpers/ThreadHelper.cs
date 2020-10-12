using System.Linq;
using System.Threading;
using System.Collections.Generic;

using GZipTest.Models;

namespace GZipTest.Helpers
{
    public class ThreadHelper
    {
        private const int MaxThreadsCount = 10;

        private int _threadCount;
        private int _nextThreadIndex;

        private readonly Dictionary<int, Thread> _threadPool = new Dictionary<int, Thread>();
        private readonly Queue<Cluster> _readyForWriting = new Queue<Cluster>();

        private readonly ManualResetEventSlim _readResetEvent = new ManualResetEventSlim(true);
        private readonly ManualResetEventSlim _writeResetEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _threadsResetEvent = new ManualResetEventSlim(false);

        public void StartThread(ThreadStart routine)
        {
            lock (_threadPool)
            {
                var thread = new Thread(routine);

                _threadPool[_threadCount++] = thread;
                thread.Start();

                if (_threadPool.Count == MaxThreadsCount)
                    _readResetEvent.Reset();
            }
        }

        public void ReleaseThread(Cluster cluster)
        {
            if (_nextThreadIndex != cluster.Index)
                WaitPreviousThreads(cluster);

            _nextThreadIndex++;
            AddToQueue(cluster);
            
            _threadsResetEvent.Set();
        }
        private void WaitPreviousThreads(Cluster cluster)
        {
            while (true)
            {
                _threadsResetEvent.Wait();

                if (_nextThreadIndex == cluster.Index)
                    return;

                _threadsResetEvent.Reset();
            }
        }
        private void AddToQueue(Cluster cluster)
        {
            lock (_readyForWriting)
            {
                _readyForWriting.Enqueue(cluster);
                _writeResetEvent.Set();
            }
        }

        public Cluster TakeCluster()
        {
            lock(_threadPool)
            lock (_readyForWriting)
            {
                var cluster = _readyForWriting.Dequeue();
                _threadPool.Remove(cluster.Index);

                _readResetEvent.Set();
                if (!_readyForWriting.Any())
                    _writeResetEvent.Reset();

                return cluster;
            }
        }

        public void WaitForReading()
        {
            _readResetEvent.Wait();
        }
        public void WaitForWriting()
        {
            _writeResetEvent.Wait();
        }

        public static void RunThread(ThreadStart routine)
        {
            new Thread(routine).Start();
        }
        public static void RunBackgroundThread(ThreadStart routine)
        {
            new Thread(routine) { IsBackground = true }.Start();
        }
    }
}
