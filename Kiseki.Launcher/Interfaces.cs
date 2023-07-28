namespace Kiseki.Launcher
{
    interface IProtocolHandler
    {
        void Register();
        void Unregister();
    }

    interface IMainWindow
    {
        void Register();
        void Unregister();
        void CreateShortcuts();
    }
}