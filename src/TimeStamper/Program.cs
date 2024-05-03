using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TimeStamper
{
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
            var processName = arguments.FirstOrDefault()?.Trim('\"');
            arguments.RemoveAt(0);

            if (!File.Exists(processName))
            {
                throw new FileNotFoundException("We can't launch a program that doesn't exist.", processName);
            }

            var configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "timestamper");
            var config = new Configuration(configDirectory);

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
            process.OutputDataReceived += (_, e) =>
            {
                if (Console.IsOutputRedirected)
                {
                    Console.WriteLine(e.Data);
                    return;
                }
                Console.Out.PrintLine(e.Data, config.StandardOutputSequence, config.TimeStampFormat);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (Console.IsErrorRedirected)
                {
                    Console.Error.WriteLine(e.Data);
                    return;
                }
                Console.Error.PrintLine(e.Data, config.StandardErrorSequence, config.TimeStampFormat);
            };
            var stopwatch = Stopwatch.StartNew();
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            stopwatch.Stop();
            
            if (!Console.IsOutputRedirected && config.ShouldOutputFooter)
            {
                Console.Out.PrintLine("=======================".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine("Exit Code: " + $"{process.ExitCode}".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine("Runtime: " + $"{stopwatch.Elapsed}".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
            }

            return process.ExitCode;
        }
    }
}
