using System.Reflection;

using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class Launcher : ILauncher
    {
        public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];
        
        public async static void Install()
        {
            Directory.CreateDirectory(Directories.Base);
            int response = await Web.CheckHealth();

            if (response != Web.RESPONSE_SUCCESS || response != Web.RESPONSE_MAINTENANCE)
            {
                if (response != Web.RESPONSE_MAINTENANCE)
                {
                    // The Kiseki website is either down or we can't connect to the internet.
                    // TODO: This is a strange scenario where we need to display an error outside of the controller. Can we do this within the page instead of a message box?
                    MessageBox.Show($"Failed to connect to the {Constants.ProjectName} website. Please check your internet connection.", Constants.ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                else
                {
                    // We are in maintenance mode, so let's ask for a license.
                    AskForLicense(Directories.License);
                    Web.LoadLicense(File.ReadAllText(Directories.License));
                    
                    // ... try this again;
                    Install();
                }
            }

            // okay, now download the launcher from the Kiseki website...
        }

        private static void AskForLicense(string licensePath)
        {
            using OpenFileDialog dialog = new()
            {
                Title = "Select your license file",
                Filter = "License files (*.bin)|*.bin",
                InitialDirectory = Directories.Base
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(dialog.FileName, licensePath, true);
            }
        }

        public static void Register()
        {
            using (RegistryKey applicationKey = Registry.CurrentUser.CreateSubKey($@"Software\{Constants.ProjectName}"))
            {
                applicationKey.SetValue("InstallLocation", Directories.Base);
            }

            using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Constants.ProjectName}");
            
            uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
            uninstallKey.SetValue("DisplayName", Constants.ProjectName);
            uninstallKey.SetValue("DisplayVersion", Version);

            if (uninstallKey.GetValue("InstallDate") is null)
                uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

            uninstallKey.SetValue("InstallLocation", Directories.Base);
            uninstallKey.SetValue("NoRepair", 1);
            uninstallKey.SetValue("Publisher", Constants.ProjectName);
            uninstallKey.SetValue("ModifyPath", $"\"{Directories.Application}\" -menu");
            uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
            uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
            uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{Constants.ProjectRepository}");
            uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{Constants.ProjectRepository}/releases/latest");
        }

        public static void Unregister()
        {
            Registry.CurrentUser.DeleteSubKey($@"Software\{Constants.ProjectName}");
        }
    }
}