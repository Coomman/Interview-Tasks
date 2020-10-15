using System;
using System.Threading;

using GZipTest.Core;
using GZipTest.Models.InputStorage;

namespace GZipTest.Models.Files
{
    public class CompressedFile
    {
        private readonly IInputStorage _inputStorage;
        private readonly ResultFile _resultFile;
        private readonly Compressor _decompressor;

        private CountdownEvent _cdEvent;

        public static event Action<int> OnStartProcessing;
        
        public CompressedFile(IInputStorage inputStorage, ResultFile resultFile, Compressor decompressor)
        {
            _inputStorage = inputStorage;
            _resultFile = resultFile;
            _decompressor = decompressor;
        }

        public void Decompress()
        {
            using (_inputStorage)
            {
                using (_decompressor)
                {
                    int chunkCount = _inputStorage.ReadInt();

                    _cdEvent = new CountdownEvent(chunkCount);
                    ThreadHelper.RunThread(() => _resultFile.WriteChunks(_cdEvent));

                    OnStartProcessing?.Invoke(chunkCount);
                    for (int i = 0; i < chunkCount; i++)
                        _decompressor.AddChunk(ReadChunk(i));

                    _cdEvent.Wait();
                }
            }
        }

        private Chunk ReadChunk(int chunkIndex)
        {
            var data = _inputStorage.ReadBytes(new byte[_inputStorage.ReadInt()]);

            return new Chunk {Data = data, Index = chunkIndex};
        }
    }
}
