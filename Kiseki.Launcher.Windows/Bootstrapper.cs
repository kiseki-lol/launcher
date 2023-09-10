namespace Kiseki.Launcher.Windows;

using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

using Kiseki.Launcher.Utilities;
using Kiseki.Launcher.Models;

using Microsoft.Win32;
using Syroot.Windows.IO;

public class Bootstrapper : Interfaces.IBootstrapper
{
    public readonly static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

    private readonly string Payload;
    private readonly Dictionary<string, string> Arguments = new();

    public event EventHandler<string>? OnHeadingChange;
    public event EventHandler<int>? OnProgressBarSet;
    public event EventHandler<Enums.ProgressBarState>? OnProgressBarStateChange;
    public event EventHandler<string[]>? OnError;

    public Bootstrapper(string payload)
    {
        Payload = payload;
    }

    public bool Initialize()
    {
        if (!Base64.IsBase64String(Payload))
        {
            return false;
        }
        
        // { mode, version, ticket, joinscript }
        string[] pieces = Base64.ConvertBase64ToString(Payload).Split("|");
        if (pieces.Length != 4)
        {
            return false;
        }

        Arguments["Mode"] = pieces[0];
        Arguments["Version"] = pieces[1];
        Arguments["Ticket"] = pieces[2];
        Arguments["JoinScript"] = pieces[3];

        return true;
    }

    public async void Run()
    {
        // Check for updates
        HeadingChange("Checking for updates...");
        
        // Check for a new launcher release from GitHub
        var launcherRelease = await Http.GetJson<GitHubRelease>($"https://api.github.com/repos/{Constants.PROJECT_REPOSITORY}/releases/latest");
        bool launcherUpToDate = true;

        // TODO: We can remove this check once we do our first release.
        if (launcherRelease is not null && launcherRelease.Assets is not null)
        {
            launcherUpToDate = Version == launcherRelease.TagName[1..];

            if (!launcherUpToDate)
            {
                // Update the launcher
                HeadingChange("Getting the latest launcher...");
                ProgressBarStateChange(Enums.ProgressBarState.Normal);

                // TODO: This needs to be rewritten. It's a mess.
                // REF: https://stackoverflow.com/a/9459441
                Thread thread = new(() => {
                    using WebClient client = new();

                    client.DownloadProgressChanged += (_, e) => {
                        double bytesIn = double.Parse(e.BytesReceived.ToString());
                        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                        double percentage = bytesIn / totalBytes * 100;

                        ProgressBarSet(int.Parse(Math.Truncate(percentage).ToString()));
                    };

                    client.DownloadFileCompleted += (_, _) => {
                        HeadingChange("Installing the latest launcher...");
                        ProgressBarStateChange(Enums.ProgressBarState.Marquee);

                        // Rename Kiseki.Launcher.exe.new -> Kiseki.Launcher.exe, and launch it with our payload
                        string command = $"del /Q \"{Paths.Application}\" && move /Y \"{Paths.Application}.new\" \"{Paths.Application}\" && start \"\" \"{Paths.Application}\" {Payload}";

                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c timeout 1 && {command}",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });

                        Environment.Exit((int)Win32.ErrorCode.ERROR_SUCCESS);
                    };

                    client.DownloadFileAsync(new Uri(launcherRelease.Assets[0].BrowserDownloadUrl), $"{Paths.Application}.new");
                });

                thread.Start();

                return;
            }
        }

        var clientRelease = await Http.GetJson<ClientRelease>(Web.Url($"/api/setup/{Arguments["Version"]}"));
        bool clientUpToDate = true;
        bool createStudioShortcut = false;

        if (clientRelease is null)
        {
            Error($"Failed to check for {Constants.PROJECT_NAME} updates", $"Failed to check for {Constants.PROJECT_NAME} updates. Please try again later.");
            return;
        }

        if (!Directory.Exists(Path.Combine(Paths.Versions, Arguments["Version"])))
        {
            Directory.CreateDirectory(Path.Combine(Paths.Versions, Arguments["Version"]));
            clientUpToDate = false;
            createStudioShortcut = true;
        }
        else
        {
            // Compute checksums of the required binaries
            for (int i = 0; i < clientRelease.Checksums.Count; i++)
            {
                string file = clientRelease.Checksums.ElementAt(i).Key;
                string checksum = clientRelease.Checksums.ElementAt(i).Value;

                if (!File.Exists(Path.Combine(Paths.Versions, Arguments["Version"], file)))
                {
                    clientUpToDate = false;
                    createStudioShortcut = true;
                    break;
                }

                using SHA256 SHA256 = SHA256.Create();
                using FileStream fileStream = File.OpenRead(Path.Combine(Paths.Versions, Arguments["Version"], file));

                string computedChecksum = Convert.ToBase64String(SHA256.ComputeHash(fileStream));

                if (checksum != computedChecksum)
                {
                    clientUpToDate = false;
                    break;
                }
            }
        }

        if (!clientUpToDate)
        {
            // Download the required binaries
            HeadingChange($"Getting the latest Kiseki {Arguments["Version"]}...");

            // Delete all files in the version directory
            Directory.Delete(Path.Combine(Paths.Versions, Arguments["Version"]), true);
            Directory.CreateDirectory(Path.Combine(Paths.Versions, Arguments["Version"]));

            // Download archive
            Task.WaitAny(Task.Factory.StartNew(async () => {
                using WebClient client = new();
                bool finished = false;

                client.DownloadProgressChanged += (_, e) => {
                    double bytesIn = double.Parse(e.BytesReceived.ToString());
                    double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    double percentage = bytesIn / totalBytes * 100;

                    ProgressBarSet(int.Parse(Math.Truncate(percentage).ToString()));
                };

                client.DownloadFileCompleted += (_, _) => finished = true;

                client.DownloadFileAsync(new Uri(clientRelease.Asset.Url), Path.Combine(Paths.Versions, Arguments["Version"], "archive.zip"));

                while (!finished) await Task.Delay(100);
            }));
            
            // Compare archive checksum
            using SHA256 SHA256 = SHA256.Create();
            using FileStream fileStream = File.OpenRead(Path.Combine(Paths.Versions, Arguments["Version"], "archive.zip"));

            string computedChecksum = Convert.ToBase64String(SHA256.ComputeHash(fileStream));

            if (clientRelease.Asset.Checksum != computedChecksum)
            {
                Error($"Failed to update {Constants.PROJECT_NAME} {Arguments["Version"]}", $"Failed to update {Constants.PROJECT_NAME}. Please try again later.");
                return;
            }

            // Extract archive
            HeadingChange($"Installing Kiseki {Arguments["Version"]}...");
            ProgressBarStateChange(Enums.ProgressBarState.Marquee);

            ZipFile.ExtractToDirectory(Path.Combine(Paths.Versions, Arguments["Version"], "archive.zip"), Path.Combine(Paths.Versions, Arguments["Version"]));
            File.Delete(Path.Combine(Paths.Versions, Arguments["Version"], "archive.zip"));
        }

        if (createStudioShortcut)
        {
            if (!Directory.Exists(Paths.StartMenu))
                Directory.CreateDirectory(Paths.StartMenu);
            
            if (File.Exists(Path.Combine(Paths.StartMenu, $"{Constants.PROJECT_NAME} Studio {Arguments["Version"]}.lnk")))
                File.Delete(Path.Combine(Paths.StartMenu, $"{Constants.PROJECT_NAME} Studio {Arguments["Version"]}.lnk"));
            
            string studioPath = Path.Combine(Paths.Versions, Arguments["Version"], $"{Constants.PROJECT_NAME}.Studio.exe");

            ShellLink.Shortcut.CreateShortcut(studioPath, "", studioPath, 0)
                .WriteToFile(Path.Combine(Paths.StartMenu, $"{Constants.PROJECT_NAME} Studio {Arguments["Version"]}.lnk"));
        }

        // We're done! Launch the game.
        HeadingChange("Launching Kiseki...");

        Process player = new()
        {
            StartInfo = new()
            {
                FileName = Path.Combine(Paths.Versions, Arguments["Version"], $"{Constants.PROJECT_NAME}.Player.exe"),
                Arguments = $"-a \"{Web.Url("/Login/Negotiate.ashx")}\" -t \"{Arguments["Ticket"]}\" -j \"{Arguments["JoinScript"]}\"",
                UseShellExecute = true,
            }
        };

        Thread waiter = new(() => {
            bool launched = false;

            while (!launched)
            {
                Thread.Sleep(100);
                launched = Win32.IsWindowVisible(player.MainWindowHandle);
            }

            Environment.Exit((int)Win32.ErrorCode.ERROR_SUCCESS);
        });

        player.Start();
        player.WaitForInputIdle();

        waiter.Start();
    }

    #region MainWindow

    protected virtual void HeadingChange(string heading)
    {
        OnHeadingChange!.Invoke(this, heading);
    }

    protected virtual void ProgressBarSet(int value)
    {
        OnProgressBarSet!.Invoke(this, value);
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

        // Create paths
        Directory.CreateDirectory(Paths.Base);
        Directory.CreateDirectory(Paths.Versions);
        Directory.CreateDirectory(Paths.Logs);
        
        // Copy ourselves
        if (!File.Exists(Paths.Application))
            File.Copy(Application.ExecutablePath, Paths.Application, true);

        // Register us and our protocol handler system-wide
        Register();
        Protocol.Register();

        // Create shortcuts
        if (!Directory.Exists(Paths.StartMenu))
            Directory.CreateDirectory(Paths.StartMenu);

        if (File.Exists(Path.Combine(Paths.StartMenu, $"Play {Constants.PROJECT_NAME}.lnk")))
            File.Delete(Path.Combine(Paths.StartMenu, $"Play {Constants.PROJECT_NAME}.lnk"));
        
        if (File.Exists(Path.Combine(Paths.Desktop, $"{Constants.PROJECT_NAME}.lnk")))
            File.Delete(Path.Combine(Paths.Desktop, $"{Constants.PROJECT_NAME}.lnk"));
        
        if (!Directory.Exists(Paths.StartMenu))
            Directory.CreateDirectory(Paths.StartMenu);
        
        ShellLink.Shortcut.CreateShortcut(Paths.Application, "", Paths.Application, 0)
            .WriteToFile(Path.Combine(Paths.StartMenu,  $"{Constants.PROJECT_NAME}.lnk"));
        
        ShellLink.Shortcut.CreateShortcut(Paths.Application, "", Paths.Application, 0)
            .WriteToFile(Path.Combine(Paths.Desktop, $"{Constants.PROJECT_NAME}.lnk"));

        // We're finished
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
        if (Directory.Exists(Paths.Logs))
            Directory.Delete(Paths.Logs, true);
        
        if (Directory.Exists(Paths.Versions))
            Directory.Delete(Paths.Versions, true);
        
        if (File.Exists(Paths.License))
            File.Delete(Paths.License);

        // Delete our shortcuts
        if (File.Exists(Path.Combine(Paths.StartMenu, $"{Constants.PROJECT_NAME}.lnk")))
            File.Delete(Path.Combine(Paths.StartMenu, $"{Constants.PROJECT_NAME}.lnk"));
        
        if (File.Exists(Path.Combine(Paths.Desktop, $"{Constants.PROJECT_NAME}.lnk")))
            File.Delete(Path.Combine(Paths.Desktop, $"{Constants.PROJECT_NAME}.lnk"));

        // Cleanup our registry entries
        Unregister();
        Protocol.Unregister();

        answer = quiet ? DialogResult.OK : MessageBox.Show($"Sucessfully uninstalled {Constants.PROJECT_NAME}!", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
        if (answer == DialogResult.OK || answer == DialogResult.Cancel)
        {
            string command = $"del /Q \"{Paths.Application}\"";

            if (Directory.GetFiles(Paths.Base, "*", SearchOption.AllDirectories).Length == 1)
            {
                // We're the only file in the directory, so we can delete the entire directory
                command += $" && rmdir \"{Paths.Base}\"";
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

        uninstallKey.SetValue("DisplayIcon", $"{Paths.Application},0");
        uninstallKey.SetValue("DisplayName", Constants.PROJECT_NAME);
        uninstallKey.SetValue("DisplayVersion", Version);

        if (uninstallKey.GetValue("InstallDate") is null)
            uninstallKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));

        uninstallKey.SetValue("InstallLocation", Paths.Base);
        uninstallKey.SetValue("Publisher", Constants.PROJECT_NAME);
        uninstallKey.SetValue("QuietUninstallString", $"\"{Paths.Application}\" -uninstall -quiet");
        uninstallKey.SetValue("UninstallString", $"\"{Paths.Application}\" -uninstall");
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
        if (!File.Exists(Paths.License))
        {
            if (!AskForLicense(Paths.License))
            {
                // User doesn't want to license this launcher
                return false;
            }
        }

        // Load the license...
        while (!Web.LoadLicense(File.ReadAllText(Paths.License)))
        {
            // ...and if it's corrupt, keep asking for a new one.
            File.Delete(Paths.License);
            MessageBox.Show($"Corrupt license file! Please verify the contents of your license file (it should be named \"license.bin\".)", Constants.PROJECT_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            AskForLicense(Paths.License, false);
        }

        return true;
    }

    public static void Unlicense()
    {
        if (File.Exists(Paths.License))
        {
            File.Delete(Paths.License);
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
                InitialDirectory = KnownFolders.Downloads.Path
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