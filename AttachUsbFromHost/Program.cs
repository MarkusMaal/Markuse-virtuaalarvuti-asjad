using Avalonia;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading;

namespace AttachUsbFromHost;

class Program
{
    public static readonly ScriptConfig scriptConfig = new();
    public static string masv_root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.masv";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();



    public static string GetStdOut(string command, bool hide_window = true)
    {
        // Start the child process.
        Process p = new();
        // Redirect the output stream of the child process.
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "ssh";
        p.StartInfo.Arguments = $"{scriptConfig.SSH_NAME}@{scriptConfig.SSH_IP} \"{command}\"";
        if (hide_window)
        {
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        }
        p.Start();
        new Thread(() =>
        {
            // wait for 15 seconds before killing SSH
            // if you get this 15 seconds timeout, it likely means the SSH connection is asking for a password
            // please setup key pairs for authentication before using this application
            Thread.Sleep(15000);
            p?.Kill();
        }).Start();
        // Do not wait for the child process to exit before
        // reading to the end of its redirected stream.
        p.WaitForExit();
        // Read the output stream first and then wait.
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return output;
    }

  
}
