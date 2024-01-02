using System;
using System.IO;
using Pastel;

namespace TimeStamper
{
    public static class ConsoleExtensions
    {
        public static void PrintLine(this TextWriter output, string line, ConsoleColor color = ConsoleColor.Green)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            output.WriteLine($"[{DateTime.Now}] ".Pastel(color) + line);
        }
    }
}
