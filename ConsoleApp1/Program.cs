using System.Diagnostics;

int exitCode = -1;
var executable = Process.GetCurrentProcess().MainModule.FileName;
var reader = new StreamReader($"{executable}.config");
var process = reader.ReadLine();
var psi = new ProcessStartInfo(process!, string.Join(" ", args))
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
    Console.Write($"[{DateTime.Now}] ");
    Console.ForegroundColor = curForeground;
    Console.WriteLine(e.Data);
};

p.EnableRaisingEvents = true;
p.Start();
p.BeginErrorReadLine();
p.BeginOutputReadLine();
p.WaitForExit();
exitCode = p.ExitCode;
Console.WriteLine($"Exit code: {exitCode}");
return exitCode;