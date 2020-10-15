using System.Linq;
using System.Threading;
using System.Collections.Generic;

using GZipTest.Models;

namespace GZipTest.Core
{
    public class ThreadHelper
    {
        private int _nextChunkIndex;

        private readonly Queue<Chunk> _readyForWriting = new Queue<Chunk>();
        private readonly List<Chunk> _waitingList = new List<Chunk>();

        private readonly ManualResetEventSlim _writeResetEvent = new ManualResetEventSlim(false);

        public void SendForWriting(Chunk chunk)
        {
            lock (_waitingList)
            {
                if (_nextChunkIndex != chunk.Index)
                {
                    _waitingList.Add(chunk);
                    return;
                }

                _nextChunkIndex++;
                AddToQueue(chunk);

                while (CheckWaitingList());
            }
        }
        private bool CheckWaitingList()
        {
            if (!_waitingList.Any())
                return false;

            for (int i = 0; i < _waitingList.Count; i++)
                if (_waitingList[i].Index == _nextChunkIndex)
                {
                    _nextChunkIndex++;
                    AddToQueue(_waitingList[i]);
                    _waitingList.RemoveAt(i);

                    return true;
                }

            return false;
        }
        private void AddToQueue(Chunk chunk)
        {
            lock (_readyForWriting)
            {
                _readyForWriting.Enqueue(chunk);
                _writeResetEvent.Set();
            }
        }

        public Chunk TakeChunk()
        {
            lock (_readyForWriting)
            {
                _readyForWriting.TryDequeue(out var chunk);

                if (!_readyForWriting.Any())
                    _writeResetEvent.Reset();

                return chunk;
            }
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
