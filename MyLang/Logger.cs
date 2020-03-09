using System;

namespace MyLang
{
    public static class Logger
    {
        public static bool LogEnabled = false;

        public static void Trace(string str)
        {
            if (LogEnabled)
            {
                Console.WriteLine(str);
            }
        }
    }
}
