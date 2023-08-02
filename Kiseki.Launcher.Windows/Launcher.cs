using System.Reflection;

using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class Launcher : ILauncher
    {
        public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];
        
        public static void Install()
        {
            Directory.CreateDirectory(Directories.Base);
        }

        // TODO: Implement this
        public static void Register()
        {
            using (RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{Constants.PROJECT_NAME}"))
            {
                applicationKey.SetValue("InstallLocation", Directories.Base);
            }

            using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Constants.PROJECT_NAME}");
            
            uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
            uninstallKey.SetValue("DisplayName", Constants.PROJECT_NAME);
            uninstallKey.SetValue("DisplayVersion", Version);

            if (uninstallKey.GetValue("InstallDate") is null)
                uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

            uninstallKey.SetValue("InstallLocation", Directories.Base);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", Constants.PROJECT_NAME);
            uninstallKey.SetValue("ModifyPath", $"\"{Directories.Application}\" -menu");
            uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
            uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
            uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{Constants.PROJECT_REPOSITORY}");
            uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{Constants.PROJECT_REPOSITORY}/releases/latest");
        }

        // TODO: Implement this
        public static void Unregister()
        {
            Registry.CurrentUser.DeleteSubKey($@"Software\{Constants.PROJECT_NAME}");
        }
    }
}