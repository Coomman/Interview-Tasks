using System.Text;
using System.Runtime.InteropServices;

namespace Technostar.External
{
    public static class ExternalFuncs
    {
        private const int SuccessResult = 777;

        public static bool ReverseString(string str, StringBuilder res)
        {
            return ProcessReverse(str, res) == SuccessResult;
        }

        [DllImport(@"..\..\..\..\x64\Debug\ExternalLib.dll", EntryPoint = "str_reverse", CallingConvention = CallingConvention.StdCall)]
        private static extern int ProcessReverse(string raw, StringBuilder res);
    }
}
