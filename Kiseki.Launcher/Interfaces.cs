namespace Kiseki.Launcher
{
    interface ILauncher
    {
        void Register();
        void Unregister();
        void CreateShortcuts();
    }
    
    interface IProtocol
    {
        void Register(string key, string name, string handler);
        void Unregister(string key);
    }
}