namespace Kiseki.Launcher.Windows;

using System.Diagnostics;
using System.Reflection;

using Microsoft.Win32;

public class Bootstrapper : Interfaces.IBootstrapper
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

    private readonly string Payload;
    private readonly Dictionary<string, string> Arguments = new();

    public event EventHandler<string>? OnHeadingChange;
    public event EventHandler<int>? OnProgressBarAdd;
    public event EventHandler<Enums.ProgressBarState>? OnProgressBarStateChange;
    public event EventHandler<string[]>? OnError;

    public Bootstrapper(string payload)
    {
        Payload = payload;
    }

    public bool Initialize()
    {
        if (!Helpers.Base64.IsBase64String(Payload))
        {
            Error($"Failed to launch {Constants.PROJECT_NAME}", $"Try launching {Constants.PROJECT_NAME} from the website again.");
            return false;
        }
        
        // { mode, version, ticket, joinscript }
        string[] pieces = Helpers.Base64.ConvertBase64ToString(Payload).Split("|");
        if (pieces.Length != 4)
        {
            Error($"Failed to launch {Constants.PROJECT_NAME}", $"Try launching {Constants.PROJECT_NAME} from the website again.");
            return false;
        }

        Arguments["Mode"] = pieces[0];
        Arguments["Version"] = pieces[1];
        Arguments["Ticket"] = pieces[2];
        Arguments["JoinScript"] = pieces[3];

        return true;
    }

    public void Run()
    {
        //
    }

    public void Abort()
    {
        //
    }

    #region MainWindow

    protected virtual void HeadingChange(string heading)
    {
        OnHeadingChange!.Invoke(this, heading);
    }

    protected virtual void ProgressBarAdd(int value)
    {
        OnProgressBarAdd!.Invoke(this, value);
    }

    protected virtual void ProgressBarStateChange(Enums.ProgressBarState state)
    {
        OnProgressBarStateChange!.Invoke(this, state);
    }

    protected virtual void Error(string heading, string text)
    {
        // Ugly hack for now (I don't want to derive EventHandler just for this)
        OnError!.Invoke(this, new string[] { heading, text });
    }

    #endregion
    #region Installation

    public static void Install()
    {
        // Cleanup our registry entries beforehand (if they even exist)
        Protocol.Unregister();
        Register();

        Directory.CreateDirectory(Directories.Base);
        Directory.CreateDirectory(Directories.Versions);
        Directory.CreateDirectory(Directories.Logs);
        
        if (!File.Exists(Directories.Application))
            File.Copy(Application.ExecutablePath, Directories.Application, true);

        Register();
        Protocol.Register();

        MessageBox.Show($"Sucessfully installed {Constants.PROJECT_NAME}!", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);

        Environment.Exit((int)Win32.ErrorCode.ERROR_SUCCESS);
    }

    public static void Uninstall(bool quiet = false)
    {
        DialogResult answer = quiet ? DialogResult.Yes : MessageBox.Show($"Are you sure you want to uninstall {Constants.PROJECT_NAME}?", Constants.PROJECT_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (answer != DialogResult.Yes)
            Environment.Exit((int)Win32.ErrorCode.ERROR_CANCELLED);

        // Close active processes
        if (Process.GetProcessesByName($"{Constants.PROJECT_NAME}.Player").Any() || Process.GetProcessesByName($"{Constants.PROJECT_NAME}.Studio").Any())
        {
            answer = quiet ? DialogResult.Yes : MessageBox.Show($"Kiseki is currently running. Would you like to close Kiseki now?", Constants.PROJECT_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
                Environment.Exit((int)Win32.ErrorCode.ERROR_CANCELLED);
            
            try
            {
                foreach (Process process in Process.GetProcessesByName($"{Constants.PROJECT_NAME}.Player"))
                    process.Kill();

                foreach (Process process in Process.GetProcessesByName($"{Constants.PROJECT_NAME}.Studio"))
                    process.Kill();
            }
            catch
            {
                Environment.Exit((int)Win32.ErrorCode.ERROR_INTERNAL_ERROR);
            }
        }

        // Delete all files
        if (Directory.Exists(Directories.Logs))
            Directory.Delete(Directories.Logs, true);
        
        if (Directory.Exists(Directories.Versions))
            Directory.Delete(Directories.Versions, true);
        
        if (File.Exists(Directories.License))
            File.Delete(Directories.License);

        // Cleanup our registry entries
        Unregister();
        Protocol.Unregister();

        answer = quiet ? DialogResult.OK : MessageBox.Show($"Sucessfully uninstalled {Constants.PROJECT_NAME}!", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
        if (answer == DialogResult.OK || answer == DialogResult.Cancel)
        {
            string command = $"del /Q \"{Directories.Application}\"";

            if (Directory.GetFiles(Directories.Base, "*", SearchOption.AllDirectories).Length == 1)
            {
                // We're the only file in the directory, so we can delete the entire directory
                command += $" && rmdir \"{Directories.Base}\"";
            }
            
            Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c timeout 5 && {command}",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            Environment.Exit((int)Win32.ErrorCode.ERROR_SUCCESS);
        }
    }

    #endregion
    #region Registration

    public static void Register()
    {            
        using RegistryKey uninstallKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Constants.PROJECT_NAME}");
        
        uninstallKey.SetValue("NoModify", 1);
        uninstallKey.SetValue("NoRepair", 1);

        uninstallKey.SetValue("DisplayIcon", $"{Directories.Application},0");
        uninstallKey.SetValue("DisplayName", Constants.PROJECT_NAME);
        uninstallKey.SetValue("DisplayVersion", Version);

        if (uninstallKey.GetValue("InstallDate") is null)
            uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

        uninstallKey.SetValue("InstallLocation", Directories.Base);
        uninstallKey.SetValue("Publisher", Constants.PROJECT_NAME);
        uninstallKey.SetValue("QuietUninstallString", $"\"{Directories.Application}\" -uninstall -quiet");
        uninstallKey.SetValue("UninstallString", $"\"{Directories.Application}\" -uninstall");
        uninstallKey.SetValue("URLInfoAbout", $"https://github.com/{Constants.PROJECT_REPOSITORY}");
        uninstallKey.SetValue("URLUpdateInfo", $"https://github.com/{Constants.PROJECT_REPOSITORY}/releases/latest");
    }

    public static void Unregister()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{Constants.PROJECT_NAME}");
        }
        catch
        {
#if DEBUG
            throw;
#endif
        }
    }

    #endregion
    #region Licensing

    public static bool License()
    {
        if (!File.Exists(Directories.License))
        {
            if (!AskForLicense(Directories.License))
            {
                // User doesn't want to license this launcher
                return false;
            }
        }

        // Load the license...
        while (!Web.LoadLicense(File.ReadAllText(Directories.License)))
        {
            // ...and if it's corrupt, keep asking for a new one.
            File.Delete(Directories.License);
            MessageBox.Show($"Corrupt license file! Please verify the contents of your license file (it should be named \"license.bin\".)", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            AskForLicense(Directories.License, false);
        }

        return true;
    }

    public static void Unlicense()
    {
        if (File.Exists(Directories.License))
        {
            File.Delete(Directories.License);
        }
    }

    private static bool AskForLicense(string licensePath, bool showDialog = true)
    {
        DialogResult answer = showDialog ? MessageBox.Show($"{Constants.PROJECT_NAME} is currently undergoing maintenance and requires a license in order to access the test site. Would you like to look for the license file now?", Constants.PROJECT_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) : DialogResult.Yes;
        
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

            return true;
        }
        
        return false;
    }

    #endregion
}