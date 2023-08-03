namespace Kiseki.Launcher.Windows;

public static class Paths
{
    public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static string StartMenu => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", Constants.PROJECT_NAME);
    public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static string Base { get; private set; } = "";
    public static string Logs { get; private set; } = "";
    public static string Versions { get; private set; } = "";
    public static string License { get; private set; } = "";
    public static string Application { get; private set; } = "";

    public static void Initialize(string baseDirectory)
    {
        Base = baseDirectory;

        if (!Directory.Exists(Base))
            Directory.CreateDirectory(Base);

        Logs = Path.Combine(Base, "Logs");
        Versions = Path.Combine(Base, "Versions");

        License = Path.Combine(Base, "License.bin");
        Application = Path.Combine(Base, $"{Constants.PROJECT_NAME}.Launcher.exe");
    }
}