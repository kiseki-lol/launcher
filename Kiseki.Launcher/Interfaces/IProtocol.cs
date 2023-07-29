namespace Kiseki.Launcher
{
    public interface IProtocol
    {
        void Register(string key, string name, string handler);
        void Unregister(string key);
    }
}