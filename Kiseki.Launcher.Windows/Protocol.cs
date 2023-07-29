using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class Protocol : IProtocol
    {
        public const string ProtocolKey = "kiseki";

        public void Register(string key, string name, string handler)
        {
            string arguments = $"\"{handler}\" \"%1\"";

            RegistryKey uriKey = Registry.CurrentUser.CreateSubKey(@$"Software\Classes\{key}");
            RegistryKey uriIconKey = uriKey.CreateSubKey("DefaultIcon");
            RegistryKey uriCommandKey = uriKey.CreateSubKey(@"shell\open\command");

            if (uriKey.GetValue("") is null)
            {
                uriKey.SetValue("", $"URL: {name} Protocol");
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

        public void Unregister(string key)
        {
            Registry.CurrentUser.DeleteSubKeyTree(@$"Software\Classes\{key}");
        }
    }
}