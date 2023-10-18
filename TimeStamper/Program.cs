using System.Diagnostics;

int exitCode = -1;
var arguments = args.ToList<string>();
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
    
    var curForeground = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"[{DateTime.Now}] ");
    Console.ForegroundColor = curForeground;
    Console.WriteLine(e.Data);
};
p.ErrorDataReceived += (s, e) =>
{
    if (string.IsNullOrEmpty(e.Data))
    {
        return;
    }

    var curForeground = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.Write($"[{DateTime.Now}] ");
    Console.ForegroundColor = curForeground;
    Console.Error.WriteLine(e.Data);
};

p.EnableRaisingEvents = true;
p.Start();
p.BeginErrorReadLine();
p.BeginOutputReadLine();
p.WaitForExit();
exitCode = p.ExitCode;
Console.WriteLine($"Exit code: {exitCode}");
return exitCode;
