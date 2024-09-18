using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TimeStamper
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var debugVar = Environment.GetEnvironmentVariable("TimeStamperDebug");
            var shouldDebug = !string.IsNullOrEmpty(debugVar) && debugVar.Equals("true", StringComparison.OrdinalIgnoreCase);
            var isRedirected = Console.IsOutputRedirected;

            if (shouldDebug)
            {
                Console.WriteLine("Waiting up to 2 minutes for debugger to be attached");
                Console.WriteLine($"Process ID: {Environment.ProcessId}");
                var sw = Stopwatch.StartNew();
                while (!Debugger.IsAttached && sw.ElapsedMilliseconds < 120000)
                {
                    Thread.Sleep(100);
                }
                
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("Debugger was not attached in time.");
                }
                else
                {
                    // Break the debugger.
                    Debugger.Break();
                }

                sw.Stop();
            }

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
            var argumentsToProcess = string.Join(' ', arguments);

            if (!File.Exists(processName))
            {
                throw new FileNotFoundException("We can't launch a program that doesn't exist.", processName);
            }

            var configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "timestamper");
            var config = new Configuration(configDirectory);

            if (!isRedirected && config.ShouldOutputHeader)
            {
                //Console.Out.PrintLine("=======================".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out
                    .PrintLine(
                        "Executable Path: " + $"{processName}".Colorize(config.InformationalSequence),
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out
                    .PrintLine(
                        "Parameters: " + string.Join(',',arguments.Select(a => a.Colorize(config.InformationalSequence))),
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out
                    .PrintLine(
                        "Parameters as passed to process: " + argumentsToProcess,
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out.PrintLine("=======================".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
            }

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(processName, argumentsToProcess)
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
                if (isRedirected)
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
            
            if (!isRedirected && config.ShouldOutputFooter)
            {
                Console.Out.PrintLine("=======================".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine("Exit Code: " + $"{process.ExitCode}".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine("Runtime: " + $"{stopwatch.Elapsed}".Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
            }

            return process.ExitCode;
        }
    }
}
