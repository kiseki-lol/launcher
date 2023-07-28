namespace Kiseki.Launcher
{
    interface IProtocolHandler
    {
        void Register(string handler);
        void Unregister();
    }

    interface IMainWindow
    {
        void Register();
        void Unregister();
        void CreateShortcuts();
    }
}