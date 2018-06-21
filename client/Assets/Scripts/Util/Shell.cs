using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
public static class ShellHelper
{
    public static string Path = ":/usr/local/bin:/usr/local/sbin";
    public static bool Sh(this string cmd, out string output)
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
        process.WaitForExit();
        if (process.ExitCode != 0) {
            output = process.StandardError.ReadToEnd();
            return false;
        } else {
            output = process.StandardOutput.ReadToEnd();
            return true;
        }
    }
    public static Regex Rgx = new Regex(@"`([^`]+)`");
    public static string EvalBackTick(this string text) 
    {
        return Rgx.Replace(text, new MatchEvaluator(((Match m) => {
            string o;
            if (!m.Groups[1].ToString().Sh(out o)) {
                throw new InvalidOperationException(o);
            }
            return o.Replace(Environment.NewLine, "");
        })));
    }
}
