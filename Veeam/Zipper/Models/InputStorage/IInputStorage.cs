using System;

namespace GZipTest.Models.InputStorage
{
    public interface IInputStorage : IDisposable
    {
        long GetFileByteSize();
        byte[] ReadBytes(byte[] bytes);
        int ReadInt();
    }
}
