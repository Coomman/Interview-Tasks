using System;
using System.Threading;

using GZipTest.Core;
using GZipTest.Models.InputStorage;

namespace GZipTest.Models.Files
{
    public class SourceFile
    {
        private const int ChunkByteSize = 1024 * 1024 * 10;
        private long _fileSizeLeft;

        private readonly IInputStorage _inputStorage;
        private readonly ResultFile _resultFile;
        private readonly Compressor _compressor;

        private CountdownEvent _cdEvent;

        public static event Action<int> OnStartProcessing;

        public SourceFile(IInputStorage inputStorage, ResultFile resultFile, Compressor compressor)
        {
            _inputStorage = inputStorage;
            _resultFile = resultFile;
            _compressor = compressor;
        }

        public void Compress()
        {
            using (_inputStorage)
            {
                using (_compressor)
                {
                    _fileSizeLeft = _inputStorage.GetFileByteSize();

                    int chunksCount = (int) Math.Ceiling(_fileSizeLeft / (double) ChunkByteSize);
                    _resultFile.WriteChunksCount(chunksCount);

                    _cdEvent = new CountdownEvent(chunksCount);
                    ThreadHelper.RunThread(() => _resultFile.WriteChunks(_cdEvent));

                    OnStartProcessing?.Invoke(chunksCount);
                    for (int i = 0; i < chunksCount; i++)
                    {
                        _compressor.AddChunk(ReadChunk(i));
                        _fileSizeLeft -= ChunkByteSize;
                    }

                    _cdEvent.Wait();
                }
            }
        }

        private Chunk ReadChunk(int chunkIndex)
        {
            var data = _inputStorage.ReadBytes(new byte[(int) Math.Min(_fileSizeLeft, ChunkByteSize)]);

            return new Chunk { Data = data, Index = chunkIndex};
        }
    }
}
