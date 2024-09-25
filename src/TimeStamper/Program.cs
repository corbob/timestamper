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
                Console.WriteLine(Strings.AwaitDebugger);
                Console.WriteLine($"{Strings.ProcessId} {Environment.ProcessId}");
                var sw = Stopwatch.StartNew();
                while (!Debugger.IsAttached && sw.ElapsedMilliseconds < 120_000)
                {
                    Thread.Sleep(100);
                }
                
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine(Strings.NoDebugger);
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
                throw new ArgumentOutOfRangeException(message: Strings.InvalidCall, innerException: null);
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
                throw new FileNotFoundException(Strings.ProcessNotFound, processName);
            }

            var configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "timestamper");
            var config = new Configuration(configDirectory);

            if (!isRedirected && config.ShouldOutputHeader)
            {
                Console.Out
                    .PrintLine(
                        $"{Strings.ProcessPath} {processName.Colorize(config.InformationalSequence)}",
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out
                    .PrintLine(
                        $"{Strings.Parameters} {string.Join(',',arguments.Select(a => a.Colorize(config.InformationalSequence)))}",
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out
                    .PrintLine(
                        $"{Strings.ParametersPassed} {argumentsToProcess}",
                        config.InformationalSequence,
                        config.TimeStampFormat);
                Console.Out.PrintLine(Strings.HorizontalRule.Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
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
                Console.Out.PrintLine(Strings.HorizontalRule.Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.ExitCode} {process.ExitCode.ToString().Colorize(config.InformationalSequence)}", config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.Duration} {stopwatch.Elapsed.ToString().Colorize(config.InformationalSequence)}", config.InformationalSequence, config.TimeStampFormat);
            }

            return process.ExitCode;
        }
    }
}
