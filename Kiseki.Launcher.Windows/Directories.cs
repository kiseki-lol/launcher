namespace Kiseki.Launcher.Windows
{
    public static class Directories
    {
        public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string Base { get; private set; } = "";
        public static string Logs { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string Application { get; private set; } = "";

        public static void Initialize(string baseDirectory)
        {
            Base = baseDirectory;

            Logs = Path.Combine(Base, "Logs");
            Versions = Path.Combine(Base, "Versions");
            
            Application = Path.Combine(Base, $"{Launcher.ProjectName}.exe");
        }
    }
}