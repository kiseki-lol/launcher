using Gtk;
using Kiseki.Launcher.Core;

using UI = Gtk.Builder.ObjectAttribute;

namespace Kiseki.Launcher.Linux
{
    public class MainWindow : Window
    {
        [UI] private readonly Image Logo = null;
        [UI] private readonly ProgressBar ProgressBar = null;
        [UI] private readonly Label PageHeading = null;
        [UI] private readonly Button CancelButton = null;

        private readonly Controller Controller;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            CancelButton.Clicked += Window_DeleteEvent;
        }

        private void Window_DeleteEvent(object? sender, EventArgs? e)
        {
            Application.Quit();
        }
    }
}
