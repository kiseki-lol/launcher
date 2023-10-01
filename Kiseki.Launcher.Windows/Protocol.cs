using Microsoft.Win32;

namespace Kiseki.Launcher.Windows;

public class Protocol : Interfaces.IProtocol
{
    public static void Register()
    {
        string arguments = $"\"{Paths.Application}\" \"%1\"";

        RegistryKey uriKey = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{Constants.PROTOCOL_KEY}");
        RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
        RegistryKey uriCommandKey = uriKey.CreateSubKey(@"shell\open\command");

        if (uriKey.GetValue("") is null)
        {
            uriKey.SetValue("", $"URL: {Constants.PROJECT_NAME} Protocol");
            uriKey.SetValue("URL Protocol", "");
        }

        if ((string?)uriCommandKey.GetValue("") != arguments)
        {
            uriCommandKey.SetValue("", arguments);
        }

        uriKey.Close();
        uriIconKey.Close();
        uriCommandKey.Close();
    }

    public static void Unregister()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{Constants.PROTOCOL_KEY}");
        }
        catch
        {
#if DEBUG
            // throw;
#endif
        }
    }
}