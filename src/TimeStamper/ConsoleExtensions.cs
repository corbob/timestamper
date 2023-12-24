using Pastel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeStamper
{
    public static class ConsoleExtensions
    {
        public static void PrintLine(this TextWriter output, string line, ConsoleColor color = ConsoleColor.Green)
        {
            output.WriteLine(string.Join(" ", $"[{DateTime.Now}]".Pastel(color), line));
        }
    }
}
