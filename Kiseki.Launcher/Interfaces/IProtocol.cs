namespace Kiseki.Launcher.Interfaces;

// This cl is responsible for handling registration of the Kiseki protcool handler
public interface IProtocol
{
    static abstract void Register();
    static abstract void Unregister();
}