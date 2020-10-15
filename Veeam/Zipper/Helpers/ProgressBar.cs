using System;
using System.Diagnostics;

namespace GZipTest.Helpers
{
    public class ProgressBar
    {
        private Stopwatch _timer;
        private int _lapsCount;
        private int _lapNum;

        public void Start(int lapsCount)
        {
            _timer = Stopwatch.StartNew();
            _lapsCount = lapsCount;

            WriteInfo(0, "00:00:00.000");
        }

        public void Lap()
        {
            if (++_lapNum > _lapsCount)
                throw new InvalidOperationException("Progress is 100%");

            var etaMs = _timer.ElapsedMilliseconds / (double) _lapNum * (_lapsCount - _lapNum);

            var eta = $"{TimeSpan.FromMilliseconds(etaMs):hh\\:mm\\:ss\\.fff}";
            var percentsDone = (int) (_lapNum / (double) _lapsCount * 100);

            Show(percentsDone, eta);
        }

        private void Show(int percentsDone, string eta)
        {
            ClearCurrentConsoleLine();
            WriteInfo(percentsDone, eta);

            if (_lapNum == _lapsCount)
                Console.WriteLine();
        }
        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        private void WriteInfo(int percentsDone, string eta)
        {
            var filledSize = percentsDone / 5;
            ConsoleColor.Green.Write($"  {percentsDone}% ");
            ConsoleColor.Green.Write($"[{new string('#', filledSize)}{new string(' ', 20 - filledSize)}]");

            ConsoleColor.Cyan.Write($"  ETA: {eta}");
            ConsoleColor.Blue.Write($"  Time elapsed: {_timer.Elapsed:hh\\:mm\\:ss\\.fff} ");
        }
    }
}
