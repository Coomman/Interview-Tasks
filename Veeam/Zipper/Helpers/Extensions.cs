using System;

namespace GZipTest.Helpers
{
    public static class Extensions
    {
        public static byte[] ToBytes(this int value)
        {
            var bytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }
        public static int ToInt(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes);
        }

        public static void Write(this ConsoleColor color, string str)
        {
            ColorizeWriting(color, str, false);
        }
        public static void WriteLine(this ConsoleColor color, string str)
        {
            ColorizeWriting(color, str, true);
        }
        private static void ColorizeWriting(ConsoleColor color, string str, bool needWriteLine)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(str);
            
            if(needWriteLine)
                Console.WriteLine();

            Console.ForegroundColor = previousColor;
        }
    }
}
