using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using DragonU3DSDK;

public class ShellExecutor : Editor
{

    public static int ExecuteShell(string path, string paramList)
    {
        int timeout = 60000;
        int exitCode = -1;

        StringBuilder output = new StringBuilder();
        StringBuilder error = new StringBuilder();

        using (System.Threading.AutoResetEvent outputWaitHandle = new System.Threading.AutoResetEvent(false))
        using (System.Threading.AutoResetEvent errorWaitHandle = new System.Threading.AutoResetEvent(false))
        {
            System.Diagnostics.Process process = null;
            using (process = new System.Diagnostics.Process())
            {
                // preparing ProcessStartInfo
                process.StartInfo.FileName = Path.GetFullPath(path);
                process.StartInfo.Arguments = paramList;
                process.StartInfo.ErrorDialog = true;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                try
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout))
                    {
                        exitCode = process.ExitCode;
                    }
                    else
                    {
                        // timed out
                        DebugUtil.LogError("wait timeout. " + timeout);
                    }
                }
                finally
                {
                    outputWaitHandle.WaitOne(timeout);
                    errorWaitHandle.WaitOne(timeout);
                }
            }
        }
        if (output.Length > 0)
        {
            DebugUtil.Log(output.ToString());
        }
        if (error.Length > 0)
        {
            DebugUtil.LogError(error.ToString());
        }

        if (exitCode != 0)
        {
            DebugUtil.LogError("ExecuteShell error: code = {0}", exitCode);
        }
        else
        {
            DebugUtil.Log("ExecuteShell success.");
        }
        return exitCode;
    }
}
