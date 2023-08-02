using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class Protocol : IProtocol
    {
        public const string PROTOCOL_KEY = "kiseki";

        public static void Register()
        {
            string arguments = $"\"{Directories.Application}\" \"%1\"";

            RegistryKey uriKey = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{PROTOCOL_KEY}");
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
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{PROTOCOL_KEY}");
        }
    }
}