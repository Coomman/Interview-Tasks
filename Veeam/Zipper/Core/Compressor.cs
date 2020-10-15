using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Collections.Concurrent;

using GZipTest.Models;

namespace GZipTest.Core
{
    public class Compressor : IDisposable
    {
        private Func<Chunk, byte[]> _op;
        private bool _disposed;

        private readonly ConcurrentQueue<Chunk> _taskQueue = new ConcurrentQueue<Chunk>();
        private readonly ThreadHelper _threadHelper;
        private readonly MaximumCountEvent _maxCountEvent;

        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);

        public Compressor(CompressionMode mode, MaximumCountEvent maxCountEvent, ThreadHelper threadHelper)
        {
            _maxCountEvent = maxCountEvent;
            _threadHelper = threadHelper;
            
            StartThreads(mode);
        }
        private void StartThreads(CompressionMode mode)
        {
            if (mode == CompressionMode.Compress)
                _op = Compress;
            else
                _op = Decompress;

            var threadsCount = Environment.ProcessorCount;
            for (int i = 0; i < threadsCount; i++)
                ThreadHelper.RunBackgroundThread(Routine);
        }

        public void AddChunk(Chunk chunk)
        {
            _maxCountEvent.Add();
            _taskQueue.Enqueue(chunk);

            _resetEvent.Set();
        }

        private void Routine()
        {
            while (!_disposed)
            {
                _resetEvent.Wait();

                if (!_taskQueue.TryDequeue(out var chunk))
                {
                    _resetEvent.Reset();
                    continue;
                }

                chunk.Data = _op(chunk);
                _threadHelper.SendForWriting(chunk);
            }
        }

        private static byte[] Compress(Chunk chunk)
        {
            using var output = new MemoryStream();
            using (var zipStream = new GZipStream(output, CompressionLevel.Optimal))
            {
                zipStream.Write(chunk.Data);
            }

            return output.ToArray();
        }
        private static byte[] Decompress(Chunk chunk)
        {
            using var input = new MemoryStream(chunk.Data);
            using var output = new MemoryStream();
            using (var zipStream = new GZipStream(input, CompressionMode.Decompress))
            {
                zipStream.CopyTo(output);
            }

            return output.ToArray();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _resetEvent?.Dispose();
        }
    }
}
