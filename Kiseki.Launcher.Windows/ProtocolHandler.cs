using System.Windows;

using Microsoft.Win32;

namespace Kiseki.Launcher.Windows
{
    public class ProtocolHandler : IProtocolHandler
    {
        public void Register(string handler)
        {
            string arguments = $"\"{handler}\" \"%1\"";

            RegistryKey uri = Registry.CurrentUser.CreateSubKey(@"Software\Classes\kiseki");
            RegistryKey icon = uri.CreateSubKey("DefaultIcon");
            RegistryKey command = uri.CreateSubKey(@"shell\open\command");

            if (uri.GetValue("") is null)
            {
                uri.SetValue("", "URL: Kiseki Protocol");
                uri.SetValue("URL Protocol", "");
            }

            if ((string?)command.GetValue("") != arguments)
            {
                command.SetValue("", arguments);
            }

            uri.Close();
            icon.Close();
            command.Close();
        }

        public void Unregister()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\kiseki");
            }
            catch (Exception)
            {
                // Key doesn't exist (and why are we here?)
#if DEBUG
                throw;
#endif
            }
        }
    }
}