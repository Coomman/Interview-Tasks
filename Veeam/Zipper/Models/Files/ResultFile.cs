using System;
using System.Threading;

using GZipTest.Core;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Models.Files
{
    public class ResultFile
    {
        private readonly IOutputStorage _outputStorage;
        private readonly ThreadHelper _threadHelper;
        private readonly MaximumCountEvent _maxCountEvent;
        private readonly bool _isCompressed;

        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        public static event Action OnEndOfIteration;

        public ResultFile(IOutputStorage outputStorage, ThreadHelper threadHelper, MaximumCountEvent maxCountEvent, bool isCompressed)
        {
            _outputStorage = outputStorage;
            _threadHelper = threadHelper;
            _maxCountEvent = maxCountEvent;
            _isCompressed = isCompressed;
        }

        public void WriteChunksCount(int chunksCount)
        {
            _outputStorage.WriteInt(chunksCount);
        }
        public void WriteChunks(CountdownEvent cdEvent)
        {
            using (_outputStorage)
            {
                while (cdEvent.CurrentCount != 0)
                {
                    _threadHelper.WaitForWriting();

                    var chunk = _threadHelper.TakeChunk().Data;
                    if (_isCompressed)
                        _outputStorage.WriteInt(chunk.Length);
                    _outputStorage.WriteBytes(chunk);

                    cdEvent.Signal();
                    _maxCountEvent.Signal();

                    GC.Collect();
                    OnEndOfIteration?.Invoke();
                }
            }

            _resetEvent.Set();
        }

        public void Wait()
        {
            _resetEvent.Wait();
        }
    }
}
