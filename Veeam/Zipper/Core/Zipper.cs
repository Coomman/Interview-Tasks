using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

using GZipTest.Helpers;
using GZipTest.Models.Files;
using GZipTest.Models.InputStorage;
using GZipTest.Models.OutputStorage;

namespace GZipTest.Core
{
    public class Zipper
    {
        // The number of chunks can be in memory simultaneously
        private const int ClusterSize = 40;

        private readonly Dictionary<string, Action> _cmd;

        private readonly StorageType _inputStorageType;
        private readonly StorageType _outputStorageType;

        private IInputStorage _inputStorage;
        private IOutputStorage _outputStorage;

        private ThreadHelper _threadHelper;
        private MaximumCountEvent _maxCountEvent;

        public Zipper(StorageType inputStorageType, StorageType outputStorageType)
        {
            _cmd = new Dictionary<string, Action>
            {
                ["compress"] = Compress,
                ["decompress"] = Decompress
            };

            _inputStorageType = inputStorageType;
            _outputStorageType = outputStorageType;

            AppDomain.CurrentDomain.FirstChanceException += ExceptionHandler;
        }
        private static void ExceptionHandler(object sender, FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine();
            ConsoleColor.Red.WriteLine(e.Exception.Message);

            Environment.Exit(1);
        }

        public void Process(string[] query)
        {
            var command = ValidateQuery(query);
            CreateHelpers(query);

            command.Invoke();
        }

        private Action ValidateQuery(IReadOnlyList<string> query)
        {
            if (query.Count != 3)
                throw new InvalidOperationException($"Wrong arguments count. Possible commands are: \"{string.Join("\\", _cmd.Keys)}\" <Source file path> <Destination file path>");

            if (!_cmd.TryGetValue(query[0].ToLower(), out var command))
                throw new InvalidOperationException($"Can't perform operation \"{query[0]}\". Possible commands are: \"{string.Join("\" \"", _cmd.Keys)}\"");

            return command;
        }
        private void CreateHelpers(IReadOnlyList<string> query)
        {
            _inputStorage = StorageFactory.GetInputStorage(_inputStorageType, query[1]);
            _outputStorage = StorageFactory.GetOutputStorage(_outputStorageType, query[2]);
            
            _threadHelper = new ThreadHelper();
            _maxCountEvent = new MaximumCountEvent(ClusterSize);
        }

        private void Compress()
        {
            var resultFile = new ResultFile(_outputStorage, _threadHelper, _maxCountEvent, true);
            var sourceFile = new SourceFile(_inputStorage, resultFile, new Compressor(CompressionMode.Compress, _maxCountEvent, _threadHelper));
            sourceFile.Compress();

            resultFile.Wait();
        }
        private void Decompress()
        {
            var resultFile = new ResultFile(_outputStorage, _threadHelper, _maxCountEvent, false);
            var compressedFile = new CompressedFile(_inputStorage, resultFile, new Compressor(CompressionMode.Decompress, _maxCountEvent, _threadHelper));
            compressedFile.Decompress();

            resultFile.Wait();
        }
    }
}
