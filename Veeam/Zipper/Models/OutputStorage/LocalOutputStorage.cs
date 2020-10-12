using System;
using System.IO;

using GZipTest.Helpers;

namespace GZipTest.Models.OutputStorage
{
    public class LocalOutputStorage : IOutputStorage
    {
        private readonly FileStream _writer;

        private bool _disposed;

        public LocalOutputStorage(string filePath)
        {
            if (File.Exists(filePath))
            {
                ConsoleColor.DarkGray.Write("File already exists, override? Print \"Yes\" to proceed: ");
                var answer = Console.ReadLine().ToLower();

                if (answer != "yes" && answer[0] != 'y')
                    throw new InvalidOperationException("Take another filename");
            }

            try
            {
                _writer = File.Create(filePath);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException($"{Path.GetFileName(filePath)} is currently in use", e);
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            _writer.Write(bytes);
        }
        public void WriteInt(int value)
        {
            _writer.Write(value.ToBytes());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _writer.Close();

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
