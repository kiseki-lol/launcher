using System.Diagnostics;

namespace Kiseki.Launcher.Windows
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string parentFolder = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            if (parentFolder.ToLower().Contains(Constants.ProjectName))
            {
                // Set to the current directory (either user-installed or default; it has "Kiseki" in the path, so that's good enough for us)
                Directories.Initialize(AppContext.BaseDirectory);
            }
            else
            {
                // Set to the default directory (user likely hasn't installed the launcher yet)
                Directories.Initialize(Path.Combine(Directories.LocalAppData, Constants.ProjectName));
            }

            Web.Initialize();

            if (!File.Exists(Directories.Application))
            {
                // The launcher is not installed, so let's install it.
                Launcher.Install();
            }

            if (args.Length == 0)
            {
                // Nothing for us to do :P
                Process.Start(Web.Url("/games"));
                Environment.Exit(0);
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow(args[0]));
        }
    }
}