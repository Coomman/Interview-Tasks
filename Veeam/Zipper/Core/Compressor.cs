using System.IO;
using System.IO.Compression;
using System.Threading;

using GZipTest.Models;

namespace GZipTest.Core
{
    public class Compressor
    {
        private readonly CountdownEvent _cdEvent;
        private readonly byte[][] _chunks;
        private readonly int _clusterNum;

        public Compressor(int chunksCount, int clusterNum)
        {
            _chunks = new byte[chunksCount][];
            _cdEvent = new CountdownEvent(chunksCount);
            _clusterNum = clusterNum;
        }

        public void Compress(Chunk chunk)
        {
            using var output = new MemoryStream();
            using (var zipStream = new GZipStream(output, CompressionLevel.Optimal))
            {
                zipStream.Write(chunk.Data);
            }

            _chunks[chunk.Index] = output.ToArray();
            _cdEvent.Signal();
        }
        public void Decompress(Chunk chunk)
        {
            using var input = new MemoryStream(chunk.Data);
            using var output = new MemoryStream();
            using (var zipStream = new GZipStream(input, CompressionMode.Decompress))
            {
                zipStream.CopyTo(output);
            }

            _chunks[chunk.Index] = output.ToArray();
            _cdEvent.Signal();
        }

        public Cluster GetCluster()
        {
            _cdEvent.Wait();

            return new Cluster {Data = _chunks, Index = _clusterNum};
        }
    }
}
