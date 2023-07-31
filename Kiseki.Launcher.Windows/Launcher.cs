using System.Reflection;

using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class Launcher : ILauncher
    {
        public const string ProjectName = "Kiseki";
        public const string ProjectRepository = "kiseki-lol/launcher";
        
        public readonly static string BaseUrl = "test.kiseki.lol"; // TODO: This should be set dynamically somehow
        public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];
        
        public static void Install()
        {
            
        }

        public static void Register()
        {
            using (RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{ProjectName}"))
            {
                applicationKey.SetValue("InstallLocation", Directories.Base);
            }

            using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{ProjectName}");
            
            uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
            uninstallKey.SetValue("DisplayName", ProjectName);
            uninstallKey.SetValue("DisplayVersion", Version);

            if (uninstallKey.GetValue("InstallDate") is null)
                uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

            uninstallKey.SetValue("InstallLocation", Directories.Base);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", ProjectName);
            uninstallKey.SetValue("ModifyPath", $"\"{Directories.Application}\" -menu");
            uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
            uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
            uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{ProjectRepository}");
            uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{ProjectRepository}/releases/latest");
        }

        public static void Unregister()
        {
            Registry.CurrentUser.DeleteSubKey($@"Software\{ProjectName}");
        }
    }
}