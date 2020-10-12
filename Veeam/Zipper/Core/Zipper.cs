using System;
using System.Collections.Generic;

using GZipTest.Helpers;
using GZipTest.Models.Files;
using GZipTest.Models.InputStorage;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Core
{
    public class Zipper
    {
        private readonly Dictionary<string, Action> _cmd;

        private readonly StorageType _inputStorageType;
        private readonly StorageType _outputStorageType;

        private IInputStorage _inputStorage;
        private IOutputStorage _outputStorage;

        public Zipper(StorageType inputStorageType, StorageType outputStorageType)
        {
            _cmd = new Dictionary<string, Action>
            {
                ["compress"] = Compress,
                ["decompress"] = Decompress
            };

            _inputStorageType = inputStorageType;
            _outputStorageType = outputStorageType;
        }

        public void Process(string[] query)
        {
            var command = ValidateQuery(query);

            command.Invoke();
        }

        private Action ValidateQuery(IReadOnlyList<string> query)
        {
            if (query.Count != 3)
                throw new InvalidOperationException($"Wrong arguments count. Possible commands are: \"{string.Join("\\", _cmd.Keys)}\" <Source file path> <Destination file path>");

            if (!_cmd.TryGetValue(query[0].ToLower(), out var command))
                throw new InvalidOperationException($"Can't perform operation \"{query[0]}\". Possible commands are: \"{string.Join("\" \"", _cmd.Keys)}\"");

            _inputStorage = StorageFactory.GetInputStorage(_inputStorageType, query[1]);
            _outputStorage = StorageFactory.GetOutputStorage(_outputStorageType, query[2]);

            return command;
        }

        private void Compress()
        {
            var sourceFile = new SourceFile(_inputStorage, _outputStorage);
            sourceFile.Compress();
        }
        private void Decompress()
        {
            var compressedFile = new CompressedFile(_inputStorage, _outputStorage);
            compressedFile.Decompress();
        }
    }
}
