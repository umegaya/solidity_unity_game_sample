using System;
using System.Diagnostics;
public static class ShellHelper
{
    public static string Path = ":/usr/local/bin:/usr/local/sbin";
    public static bool Sh(this string cmd, out string err)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var si = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"" + escapedArgs + "\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        si.EnvironmentVariables["PATH"] += Path;
        var process = new Process()
        {
            StartInfo = si
        };
        process.Start();
        err = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) {
            return false;
        } else {
            return true;
        }
    }
}
