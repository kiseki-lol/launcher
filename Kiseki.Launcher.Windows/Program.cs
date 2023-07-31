namespace Kiseki.Launcher.Windows
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string parentFolder = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            Directories.Initialize(parentFolder.ToLower().Contains(Launcher.ProjectName) ? AppContext.BaseDirectory : Path.Combine(Directories.LocalAppData, Launcher.ProjectName));

            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow(args));
        }
    }
}