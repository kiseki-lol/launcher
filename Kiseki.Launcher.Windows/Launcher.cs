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
            int response = Web.CheckHealth();
            
            if (response != Web.RESPONSE_SUCCESS)
            {
                if (response != Web.RESPONSE_MAINTENANCE)
                {
                    // The Kiseki website is either down or we can't connect to the internet.
                    MessageBox.Show($"Failed to connect to the {Constants.PROJECT_NAME} website. Please check your internet connection.", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                else
                {
                    // We are in maintenance mode, so let's ask for a license.
                    if (!File.Exists(Directories.License))
                    {
                        AskForLicense(Directories.License);
                    }

                    // ... load the license ...
                    while (!Web.LoadLicense(File.ReadAllText(Directories.License)))
                    {
                        // ... and if it's invalid, keep asking for a new one.
                        File.Delete(Directories.License);
                        MessageBox.Show($"Corrupt license file! Please verify the contents of your license file (it should be named \"license.bin\".)", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AskForLicense(Directories.License, false);
                    }
                    
                    // ... and then try this again;
                    Install();
                }
            }

            // okay, now download the launcher from the Kiseki website...
        }

        private static void AskForLicense(string licensePath, bool showDialog = true)
        {
            DialogResult answer = showDialog ? MessageBox.Show($"{Constants.PROJECT_NAME} is currently under maintenance and requires a license in order to access games. Would you like to look for the license file now?", Constants.PROJECT_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) : DialogResult.Yes;
            
            if (answer == DialogResult.Yes)
            {
                using OpenFileDialog dialog = new()
                {
                    Title = "Select your license file",
                    Filter = "License files (*.bin)|*.bin",
                    InitialDirectory = Win32.GetDownloadsPath()
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(dialog.FileName, licensePath, true);
                }
            }
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