using System;
using System.Threading;

using GZipTest.Core;
using GZipTest.Helpers;
using GZipTest.Models.InputStorage;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Models.Files
{
    public class SourceFile
    {
        private const int ChunkByteSize = 1024 * 1024 * 10;
        private const int ClusterSize = 10;

        private readonly IInputStorage _inputStorage;
        private readonly IOutputStorage _outputStorage;

        private CountdownEvent _cdEvent;
        private readonly ThreadHelper _threadHelper = new ThreadHelper();

        public static event Action<int> OnStartProcessing;
        public static event Action OnEndOfIteration; 

        public SourceFile(IInputStorage inputStorage, IOutputStorage outputStorage)
        {
            _inputStorage = inputStorage;
            _outputStorage = outputStorage;
        }

        public void Compress()
        {
            using (_inputStorage)
            {
                using (_outputStorage)
                {
                    long fileSizeLeft = _inputStorage.GetFileByteSize();
                    long chunksLeft = (long) Math.Ceiling(fileSizeLeft / (double) ChunkByteSize);
                    int clustersCount = (int) Math.Ceiling(chunksLeft / (double) ClusterSize);

                    _outputStorage.WriteInt(clustersCount);

                    _cdEvent = new CountdownEvent(clustersCount);
                    ThreadHelper.RunBackgroundThread(WriteCompressedChunks);

                    OnStartProcessing?.Invoke(clustersCount);
                    for (int i = 0; i < clustersCount; i++)
                    {
                        ReadCluster(chunksLeft, fileSizeLeft, i);
                        chunksLeft -= ClusterSize;
                        fileSizeLeft -= ClusterSize * ChunkByteSize;

                        OnEndOfIteration?.Invoke();
                    }

                    _cdEvent.Wait();
                }
            }
        }

        private void ReadCluster(long chunksLeft, long fileSizeLeft, int clusterNum)
        {
            _threadHelper.WaitForReading();

            var clusterSize = (int) Math.Min(chunksLeft, ClusterSize);
            var compressor = new Compressor(clusterSize, clusterNum);

            for (long i = 0; i < clusterSize; i++)
            {
                var chunk = new Chunk { Data = ReadSourceChunk(fileSizeLeft - i * ChunkByteSize), Index = (int)(i % clusterSize) };
                ThreadHelper.RunThread(() => compressor.Compress(chunk));
            }

            _threadHelper.StartThread(() => _threadHelper.ReleaseThread(compressor.GetCluster()));
        }
        private byte[] ReadSourceChunk(long bytesLeft)
        {
            var chunk = new byte[(int) Math.Min(bytesLeft, ChunkByteSize)];

            return _inputStorage.ReadBytes(chunk);
        }

        private void WriteCompressedChunks()
        {
            while (true)
            {
                _threadHelper.WaitForWriting();

                var cluster = _threadHelper.TakeCluster();
                WriteHeader(cluster.Data);
                WriteChunks(cluster.Data);

                _cdEvent.Signal();
                GC.Collect();
            }
        }
        private void WriteHeader(byte[][] chunks)
        {
            _outputStorage.WriteInt(chunks.Length);

            foreach (var chunk in chunks)
                _outputStorage.WriteInt(chunk.Length);
        }
        private void WriteChunks(byte[][] chunks)
        {
            foreach (var chunk in chunks)
                _outputStorage.WriteBytes(chunk);
        }
    }
}
