using System;
using System.IO;

using GZipTest.Helpers;

namespace GZipTest.Models.InputStorage
{
    public sealed class LocalInputStorage : IInputStorage
    {
        private readonly FileStream _reader;

        private bool _disposed;

        public LocalInputStorage(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Can't find {Path.GetFileName(filePath)}", filePath);

            try
            {
                _reader = File.OpenRead(filePath);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException($"{Path.GetFileName(filePath)} is currently in use", e);
            }
        }

        public long GetFileByteSize()
        {
            return new FileInfo(_reader.Name).Length;
        }
        public byte[] ReadBytes(byte[] bytes)
        {
            _reader.Read(bytes);
            return bytes;
        }
        public int ReadInt()
        {
            var buffer = new byte[4];

            if (_reader.Read(buffer) != 4)
                throw new EndOfStreamException("The end of file");

            return buffer.ToInt();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _reader.Close();

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
