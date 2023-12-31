using System;
using System.IO;

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

            Console.ForegroundColor = color;
            output.Write($"[{DateTime.Now}] ");
            Console.ResetColor();
            output.WriteLine(line);
        }
    }
}
