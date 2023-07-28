namespace Kiseki.Launcher
{
    interface IProtocolHandler
    {
        void Register(string key, string name, string handler);
        void Unregister(string key);
    }

    interface IMainWindow
    {
        void Register();
        void Unregister();
        void CreateShortcuts();
    }
}