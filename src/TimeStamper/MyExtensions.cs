using System;
using System.IO;

namespace TimeStamper
{
    public static class MyExtensions
    {
        public static void PrintLine(this TextWriter output, string line, string colorEscape, string timeFormat)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            output.WriteLine(DateTime.Now.ToString(timeFormat).Colorize(colorEscape) + line);
        }

        public static string Colorize(this string input, string colorEscape)
        {
            return $"\x1b[{colorEscape}m{input}\x1b[0m";
        }
    }
}
