using System;

namespace GZipTest.Models.OutputStorage
{
    public interface IOutputStorage: IDisposable
    {
        void WriteBytes(byte[] bytes);
        void WriteInt(int value);
    }
}
