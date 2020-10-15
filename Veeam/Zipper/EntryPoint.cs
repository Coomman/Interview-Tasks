using GZipTest.Core;
using GZipTest.Helpers;
using GZipTest.Models.Files;

namespace GZipTest
{
    internal class EntryPoint
    {
        private static void Main(string[] query)
        {
            AddProgressBar();

            var zipper = new Zipper(StorageType.Local, StorageType.Local);
            zipper.Process(query);
        }

        private static void AddProgressBar()
        {
            var progressBar = new ProgressBar();

            SourceFile.OnStartProcessing += progressBar.Start;
            CompressedFile.OnStartProcessing += progressBar.Start;

            ResultFile.OnEndOfIteration += progressBar.Lap;
        }
    }
}
