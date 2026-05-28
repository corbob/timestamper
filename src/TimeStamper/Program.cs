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
                    Thread.Sleep(100);

                if (!Debugger.IsAttached)
                    Console.WriteLine(Strings.NoDebugger);
                else
                    Debugger.Break();

                sw.Stop();
            }

            if (args.Length == 0)
                throw new ArgumentOutOfRangeException(message: Strings.InvalidCall, innerException: null);

            var arguments = args.Select(a => a != null && a.Contains(' ') ? $"\"{a}\"" : a).ToList();
            var processName = arguments.FirstOrDefault()?.Trim('\"');
            arguments.RemoveAt(0);
            var argumentsToProcess = string.Join(' ', arguments);

            if (!File.Exists(processName))
                throw new FileNotFoundException(Strings.ProcessNotFound, processName);

            var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "timestamper");
            var config = new Configuration(configDirectory);

            if (!isRedirected && config.ShouldOutputHeader)
            {
                Console.Out.PrintLine($"{Strings.ProcessPath} {processName.Colorize(config.InformationalSequence)}", config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.Parameters} {string.Join(',', arguments.Select(a => a.Colorize(config.InformationalSequence)))}", config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.ParametersPassed} {argumentsToProcess}", config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine(Strings.HorizontalRule.Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
            }

            var stopwatch = Stopwatch.StartNew();
            int exitCode;

            if (!isRedirected && OperatingSystem.IsWindows())
                exitCode = RunWithPseudoConsole(processName, argumentsToProcess, config, stopwatch);
            else
                exitCode = RunWithRedirect(processName, argumentsToProcess, isRedirected, config, stopwatch);

            if (!isRedirected && config.ShouldOutputFooter)
            {
                Console.Out.PrintLine(Strings.HorizontalRule.Colorize(config.InformationalSequence), config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.ExitCode} {exitCode.ToString().Colorize(config.InformationalSequence)}", config.InformationalSequence, config.TimeStampFormat);
                Console.Out.PrintLine($"{Strings.Duration} {stopwatch.Elapsed.ToString().Colorize(config.InformationalSequence)}", config.InformationalSequence, config.TimeStampFormat);
            }

            return exitCode;
        }

        private static int RunWithPseudoConsole(string processName, string arguments, Configuration config, Stopwatch stopwatch)
        {
            short width, height;
            try { width = (short)Console.WindowWidth; height = (short)Console.WindowHeight; }
            catch { width = 120; height = 30; }

            using var inputPipe = new PseudoConsolePipe();
            using var outputPipe = new PseudoConsolePipe();
            var pseudoConsole = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, width, height);

            var processInfo = ProcessFactory.Start(processName, arguments, pseudoConsole);

            // Close our copy of the write end so reading from outputPipe.ReadSide yields EOF when the process exits
            outputPipe.WriteSide.Close();

            // Pump our stdin into the pseudo console's input pipe
            var stdinThread = new Thread(() =>
            {
                try
                {
                    using var inputStream = new FileStream(inputPipe.WriteSide, FileAccess.Write);
                    Console.OpenStandardInput().CopyTo(inputStream);
                }
                catch { /* process exited */ }
            });
            stdinThread.IsBackground = true;
            stdinThread.Start();

            // Read timestamped output from the pseudo console's output pipe
            var outputThread = new Thread(() =>
            {
                try
                {
                    using var outputStream = new FileStream(outputPipe.ReadSide, FileAccess.Read);
                    using var reader = new StreamReader(outputStream);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        Console.Out.PrintLine(line, config.StandardOutputSequence, config.TimeStampFormat);
                }
                catch { /* pipe closed */ }
            });
            outputThread.Start();

            NativeMethods.WaitForSingleObject(processInfo.hProcess, NativeMethods.INFINITE);
            stopwatch.Stop();

            // Closing the pseudo console signals EOF on the output pipe
            pseudoConsole.Dispose();
            outputThread.Join();

            NativeMethods.GetExitCodeProcess(processInfo.hProcess, out uint exitCode);
            NativeMethods.CloseHandle(processInfo.hProcess);

            return (int)exitCode;
        }

        private static int RunWithRedirect(string processName, string arguments, bool isRedirected, Configuration config, Stopwatch stopwatch)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(processName, arguments)
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
                if (isRedirected) { Console.WriteLine(e.Data); return; }
                Console.Out.PrintLine(e.Data, config.StandardOutputSequence, config.TimeStampFormat);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (Console.IsErrorRedirected) { Console.Error.WriteLine(e.Data); return; }
                Console.Error.PrintLine(e.Data, config.StandardErrorSequence, config.TimeStampFormat);
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            stopwatch.Stop();
            return process.ExitCode;
        }
    }
}
