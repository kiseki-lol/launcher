namespace Kiseki.Launcher
{
    public interface ILauncher
    {
        static abstract void Install();
        static abstract void Uninstall(bool quiet = false);
        static abstract void Register();
        static abstract void Unregister();
    }
}