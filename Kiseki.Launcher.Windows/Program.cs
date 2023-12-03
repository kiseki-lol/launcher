using System.Diagnostics;

namespace Kiseki.Launcher.Windows;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-uninstall")
        {
            Bootstrapper.Uninstall(args[0] == "-quiet");
            return;
        }

        // Initialize directories
        if (Path.GetFileName(Path.GetDirectoryName(Application.ExecutablePath))!.ToLower().Contains(Constants.PROJECT_NAME.ToLower()))
        {
            // Set to the current directory (user likely has installed the launcher, seeing as parent folder name contains the project name)
            Paths.Initialize(Path.GetDirectoryName(Application.ExecutablePath)!);
        }
        else
        {
            // Set to the default directory (user likely hasn't installed the launcher yet)
            Paths.Initialize(Path.Combine(Paths.LocalAppData, Constants.PROJECT_NAME));
        }

        Web.Initialize();

        if (!Web.IsConnected && Web.IsInMaintenance)
        {
            // Try licensing this launcher and attempt to connect again
            if (!Bootstrapper.License())
            {
                return;
            }

            Web.Initialize();
        }

        if (!Web.IsConnected)
        {
            if (Web.IsInMaintenance)
            {
                // Unlicense this launcher
                Bootstrapper.Unlicense();
            }

            MessageBox.Show($"Failed to connect to {Constants.PROJECT_NAME}. Please check your internet connection and try again.", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!File.Exists(Paths.Application))
        {
            Bootstrapper.Install();
            return;
        }

        if (args.Length == 0)
        {
            // Nothing for us to do :P
            Process.Start(new ProcessStartInfo()
            {
                FileName = Web.FormatUrl("/games"),
                UseShellExecute = true
            });

            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow(args[0]));
    }
}