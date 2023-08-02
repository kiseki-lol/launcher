using System.Diagnostics;

namespace Kiseki.Launcher.Windows
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string parentFolder = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            if (parentFolder.ToLower().Contains(Constants.PROJECT_NAME))
            {
                // Set to the current directory (either user-installed or default; it has "Kiseki" in the path, so that's good enough for us)
                Directories.Initialize(AppContext.BaseDirectory);
            }
            else
            {
                // Set to the default directory (user likely hasn't installed the launcher yet)
                Directories.Initialize(Path.Combine(Directories.LocalAppData, Constants.PROJECT_NAME));
            }

            if (!Web.Initialize())
            {
                MessageBox.Show($"Failed to connect to {Constants.PROJECT_NAME}. Please check your internet connection and try again.", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(Directories.Application))
            {
                // The launcher is not installed, so let's run the install process - this will also exit the application
                Launcher.Install();
            }
            else
            {
                if (args.Length == 0)
                {
                    // Nothing for us to do :P
                    Process.Start(Web.Url("/games"));
                    return;
                }

                ApplicationConfiguration.Initialize();
                Application.Run(new MainWindow(args[0]));
            }
        }
    }
}