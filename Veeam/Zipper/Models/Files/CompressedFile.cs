using System;
using System.Threading;

using GZipTest.Core;
using GZipTest.Helpers;
using GZipTest.Models.InputStorage;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Models.Files
{
    public class CompressedFile
    {
        private readonly IInputStorage _inputStorage;
        private readonly IOutputStorage _outputStorage;

        private CountdownEvent _cdEvent;
        private readonly ThreadHelper _threadHelper = new ThreadHelper();

        public static event Action<int> OnStartProcessing;
        public static event Action OnEndOfIteration;

        public CompressedFile(IInputStorage inputStorage, IOutputStorage outputStorage)
        {
            _inputStorage = inputStorage;
            _outputStorage = outputStorage;
        }

        public void Decompress()
        {
            using (_inputStorage)
            {
                using (_outputStorage)
                {
                    int clustersCount = _inputStorage.ReadInt();

                    _cdEvent = new CountdownEvent(clustersCount);
                    ThreadHelper.RunBackgroundThread(WriteDecompressedChunks);

                    OnStartProcessing?.Invoke(clustersCount);
                    for (int i = 0; i < clustersCount; i++)
                    {
                        ReadCluster(i);
                        OnEndOfIteration?.Invoke();
                    }

                    _cdEvent.Wait();
                }
            }
        }

        private void ReadCluster(int index)
        {
            _threadHelper.WaitForReading();

            var header = ReadHeader();

            var decompressor = new Compressor(header.Length, index);

            for (int i = 0; i < header.Length; i++)
            {
                var chunk = new Chunk { Data = _inputStorage.ReadBytes(new byte[header[i]]), Index = i };
                ThreadHelper.RunThread(() => decompressor.Decompress(chunk));
            }

            _threadHelper.StartThread(() => _threadHelper.ReleaseThread(decompressor.GetCluster()));
        }
        private int[] ReadHeader()
        {
            var header = new int[_inputStorage.ReadInt()];

            for (int i = 0; i < header.Length; i++)
                header[i] = _inputStorage.ReadInt();

            return header;
        }

        private void WriteDecompressedChunks() 
        {
            while (true)
            {
                _threadHelper.WaitForWriting();

                var cluster = _threadHelper.TakeCluster().Data;

                foreach (var chunk in cluster)
                    _outputStorage.WriteBytes(chunk);

                _cdEvent.Signal();
                GC.Collect();
            }
        }
    }
}
