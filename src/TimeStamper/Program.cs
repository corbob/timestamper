using System.Diagnostics;
using TimeStamper;

var stopwatch = Stopwatch.StartNew();
int exitCode = -1;
var arguments = args.Select(a =>
{
    if (a != null && a.IndexOf(' ') != -1)
    {
        return $"\"{a}\"";
    }
    return a;
}).ToList<string>();

if (arguments.Count == 0)
{
    throw new ArgumentNullException(nameof(arguments));
}

var process = arguments.FirstOrDefault();
arguments.RemoveAt(0);

if (!System.IO.File.Exists(process))
{
    throw new FileNotFoundException("We can't launch a program that doesn't exist.", process);
}

var psi = new ProcessStartInfo(process, string.Join(" ", arguments))
{
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true,
};

using var p = new Process();
p.StartInfo = psi;
p.OutputDataReceived += (s, e) =>
{
    if (string.IsNullOrEmpty(e.Data))
    {
        return;
    }
    
    Console.Out.PrintLine(e.Data);
};
p.ErrorDataReceived += (s, e) =>
{
    if (string.IsNullOrEmpty(e.Data))
    {
        return;
    }

    Console.Error.PrintLine(e.Data, ConsoleColor.Red);
};

p.EnableRaisingEvents = true;
p.Start();
p.BeginErrorReadLine();
p.BeginOutputReadLine();
p.WaitForExit();
stopwatch.Stop();
exitCode = p.ExitCode;
Console.Out.PrintLine($"Exit Code: {exitCode}");
Console.Out.PrintLine($"Runtime: {stopwatch.Elapsed}");
return exitCode;
