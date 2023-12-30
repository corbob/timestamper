using System;
using System.IO;

namespace TimeStamper
{
    public static class ConsoleExtensions
    {
        public static void PrintLine(this TextWriter output, string line)
        {
            output.WriteLine($"[{DateTime.Now}] {line}");
        }
    }
}
