using System.IO;
using System.Linq;

using NUnit.Framework;
using FluentAssertions;

using GZipTest.Core;
using GZipTest.Helpers;

namespace ZipperTests
{
    public class CompressionTests
    {
        private const string TestFolder = "testdata\\test";
        private const string ResultPath = "res.cmp";
        private const string ResultPath2 = "res2.cmp";

        private readonly Zipper _zipper = new Zipper(StorageType.Local, StorageType.Local);

        [TearDown]
        public void TearDown()
        {
            File.Delete(ResultPath);
            File.Delete(ResultPath2);
        }

        private void TestRoutine(string sourceFile)
        {
            _zipper.Process(GetQuery(sourceFile, ResultPath, false));
            _zipper.Process(GetQuery(ResultPath, ResultPath2, true));

            File.ReadAllBytes(sourceFile).SequenceEqual(File.ReadAllBytes(ResultPath2)).Should().BeTrue();
        }
        private void RunTest(string extension)
        {
            var sourceFilePath = $"{TestFolder}{extension}";

            TestRoutine(sourceFilePath);
        }

        [Test]
        public void FullTest_Cr2()
        {
            RunTest(".cr2");
        }
        [Test]
        public void FullTest_Mp4()
        {
            RunTest(".mp4");
        }
        [Test]
        public void FullTest_Mkv()
        {
            RunTest(".mkv");
        }
        [Test]
        public void FullTest_Zero()
        {
            RunTest(".zer0");
        }

        public static string[] GetQuery(string sourceFile, string resultFile, bool isCompressed)
            => new[] { isCompressed ? "decompress" : "compress", sourceFile, resultFile };
    }
}