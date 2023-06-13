using Gtk;

namespace Kiseki.Launcher.Linux
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.Kiseki.Launcher", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var window = new MainWindow();
            app.AddWindow(window);

            window.Show();
            Application.Run();
        }
    }
}