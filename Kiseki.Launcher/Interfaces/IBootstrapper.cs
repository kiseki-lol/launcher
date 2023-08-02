namespace Kiseki.Launcher
{
    public interface IBootstrapper
    {
        // These connect to MainWindow
        event EventHandler<string>? OnHeadingChange;
        event EventHandler<int>? OnProgressBarAdd;
        event EventHandler<Enums.ProgressBarState>? OnProgressBarStateChange;
        event EventHandler<string[]>? OnError;
        
        // Actual bootstrapping
        bool Initialize();
        void Run();
        void Abort();

        // Installation (i.e. putting the launcher in the Kiseki folder)
        static abstract void Install();
        static abstract void Uninstall(bool quiet = false);

        // Registering the launcher onto the system (as well as creating shortcuts)
        static abstract void Register();
        static abstract void Unregister();

        // Licensing the launcher
        static abstract void License();
        static abstract void Unlicense();
    }
}