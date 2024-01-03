using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TimeStamper
{
    [PublicAPI]
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentOutOfRangeException(message: "TimeStamper must be called with the executable path to run.", innerException: null);
            }

            var arguments = args.Select(
                a =>
                {
                    if (a != null && a.Contains(' '))
                    {
                        return $"\"{a}\"";
                    }
                    return a;
                })
                .ToList();

            // Process Name should not contain quotes
            var processName = arguments.FirstOrDefault().Trim('\"');
            arguments.RemoveAt(0);

            if (!File.Exists(processName))
            {
                throw new FileNotFoundException("We can't launch a program that doesn't exist.", processName);
            }

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(processName, string.Join(" ", arguments))
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (_, e) => Console.Out.PrintLine(e.Data);
            process.ErrorDataReceived += (_, e) => Console.Error.PrintLine(e.Data, ConsoleColor.Red);
            var stopwatch = Stopwatch.StartNew();
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            stopwatch.Stop();
            Console.Out.PrintLine($"Exit Code: {process.ExitCode}");
            Console.Out.PrintLine($"Runtime: {stopwatch.Elapsed}");
            return process.ExitCode;
        }
    }
}
