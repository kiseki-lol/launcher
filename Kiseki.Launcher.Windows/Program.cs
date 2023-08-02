using System.Diagnostics;

namespace Kiseki.Launcher.Windows;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Initialize directories
        string parentFolder = Path.GetDirectoryName(Application.StartupPath)!;

        if (Path.GetDirectoryName(parentFolder)!.ToLower().Contains(Constants.PROJECT_NAME.ToLower()))
        {
            // Set to the current directory (user likely has installed the launcher, seeing as parent folder name contains the project name)
            Directories.Initialize(parentFolder);
        }
        else
        {
            // Set to the default directory (user likely hasn't installed the launcher yet)
            Directories.Initialize(Path.Combine(Directories.LocalAppData, Constants.PROJECT_NAME));
        }

        bool isConnected = Web.Initialize();
        if (!isConnected && Web.IsInMaintenance)
        {
            // Try licensing this launcher and attempt to connect again
            Bootstrapper.License();
            isConnected = Web.Initialize();
        }

        if (!isConnected)
        {
            if (Web.IsInMaintenance)
            {
                // Unlicense this launcher
                Bootstrapper.Unlicense();
            }

            MessageBox.Show($"Failed to connect to {Constants.PROJECT_NAME}. Please check your internet connection and try again.", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!File.Exists(Directories.Application))
        {
            Bootstrapper.Install();
            return;
        }

        if (args.Length == 0)
        {
            // Nothing for us to do :P
            Process.Start(new ProcessStartInfo()
            {
                FileName = Web.Url("/games"),
                UseShellExecute = true
            });

            return;
        }

        if (args[0] == "-uninstall")
        {
            Bootstrapper.Uninstall(args[0] == "-quiet");
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow(args[0]));
    }
}