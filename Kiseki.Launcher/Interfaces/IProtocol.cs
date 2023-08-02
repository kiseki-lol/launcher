namespace Kiseki.Launcher
{
    public interface IProtocol
    {
        static abstract void Register();
        static abstract void Unregister();
    }
}