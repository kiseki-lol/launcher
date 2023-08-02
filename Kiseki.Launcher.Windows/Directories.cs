namespace Kiseki.Launcher.Windows
{
    public static class Directories
    {
        public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string Base { get; private set; } = "";
        public static string Logs { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string License { get; private set; } = "";
        public static string Application { get; private set; } = "";

        public static void Initialize(string baseDirectory)
        {
            Base = baseDirectory;

            if (!Directory.Exists(Base))
            {
                Directory.CreateDirectory(Base); // just in case
            }

            Logs = Path.Combine(Base, "Logs");
            Versions = Path.Combine(Base, "Versions");

            License = Path.Combine(Base, "license.bin");
            Application = Path.Combine(Base, $"{Constants.PROJECT_NAME}.Launcher.exe");
        }
    }
}