using Kiseki.Launcher.Windows.Properties;

namespace Kiseki.Launcher.Windows
{
    [System.ComponentModel.DesignerCategory("")]
    public class MainWindow : Form
    {
        private readonly TaskDialogButton CloseButton;
        private readonly TaskDialogPage Page;
        private readonly Controller Controller;

        public MainWindow(string[] args)
        {
            this.CloseButton = TaskDialogButton.Close;

            this.Page = new TaskDialogPage()
            {
                Caption = "Kiseki",
                AllowMinimize = true,
            
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                },

                Buttons = { this.CloseButton }
            };

            this.Controller = new Launcher.Controller("kiseki.lol", args);
            this.Controller.PageHeadingChanged += Controller_PageHeadingChanged;
            this.Controller.ProgressBarChanged += Controller_ProgressBarChanged;
            this.Controller.ProgressBarStateChanged += Controller_ProgressBarStateChanged;
            this.Controller.Launched += Controller_Launched;
            
            this.Page.Destroyed += (s, e) =>
            {
                this.Controller.Dispose();
                Environment.Exit(0);
            };

            this.ShowProgressDialog();
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Controller_PageHeadingChanged(object sender, string Heading)
        {
            this.Page.Heading = Heading;
        }

        private void Controller_ProgressBarChanged(object sender, int Value)
        {
            this.Page.ProgressBar.Value = Value;
        }

        private void Controller_ProgressBarStateChanged(object sender, ProgressBarState State)
        {
            this.Page.ProgressBar.State = State switch
            {
                ProgressBarState.Normal => TaskDialogProgressBarState.Normal,
                ProgressBarState.Marquee => TaskDialogProgressBarState.Marquee,
                _ => throw new NotImplementedException()
            };
        }

        private void Controller_Launched(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void ShowProgressDialog()
        {
            TaskDialogIcon logo = new(Resources.IconKiseki);
            this.Page.Icon = logo;

            this.Page.Created += (s, e) =>
            {
                this.Controller.Start();
            };

            TaskDialog.ShowDialog(this.Page);
        }
    }
}
