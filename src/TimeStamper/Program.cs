using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TimeStamper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            int exitCode = -1;
            var arguments = args.Select(
                a =>
                {
                    if(a != null && a.IndexOf(' ') != -1)
                    {
                        return $"\"{a}\"";
                    }
                    return a;
                })
                .ToList<string>();

            if (arguments.Count == 0)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var processName = arguments.FirstOrDefault();
            arguments.RemoveAt(0);

            if (!File.Exists(processName))
            {
                throw new FileNotFoundException("We can't launch a program that doesn't exist.", processName);
            }

            var psi = new ProcessStartInfo(processName, string.Join(" ", arguments))
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var process = new Process();
            process.StartInfo = psi;
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                Console.Out.PrintLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                Console.Error.PrintLine(e.Data);
            };

            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            stopwatch.Stop();
            exitCode = process.ExitCode;
            Console.Out.PrintLine($"Exit Code: {exitCode}");
            Console.Out.PrintLine($"Runtime: {stopwatch.Elapsed}");
            return exitCode;
        }
    }
}
