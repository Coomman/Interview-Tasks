using System;
using GZipTest.Models.InputStorage;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Helpers
{
    public enum StorageType { Local };

    public static class StorageFactory
    {
        public static IInputStorage GetInputStorage(StorageType type, string filePath)
        {
            return type switch
            {
                StorageType.Local => new LocalInputStorage(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown storage type")
            };
        }

        public static IOutputStorage GetOutputStorage(StorageType type, string filePath)
        {
            return type switch
            {
                StorageType.Local => new LocalOutputStorage(filePath),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown storage type")
            };
        }
    }
}
