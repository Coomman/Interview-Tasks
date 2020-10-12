using System;

using GZipTest.Core;
using GZipTest.Helpers;
using GZipTest.Models.Files;

namespace GZipTest
{
    internal class EntryPoint
    {
        private static int Main(string[] query)
        {
            try
            {
                AddProgressBar();

                var zipper = new Zipper(StorageType.Local, StorageType.Local);
                zipper.Process(query);
            }
            catch (Exception e)
            {
                ConsoleColor.Red.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }

        private static void AddProgressBar()
        {
            var progressBar = new ProgressBar();

            SourceFile.OnStartProcessing += progressBar.Start;
            SourceFile.OnEndOfIteration += progressBar.Lap;

            CompressedFile.OnStartProcessing += progressBar.Start;
            CompressedFile.OnEndOfIteration += progressBar.Lap;
        }
    }
}
