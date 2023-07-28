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
            CloseButton = TaskDialogButton.Close;

            Page = new TaskDialogPage()
            {
                Caption = "Kiseki",
                AllowMinimize = true,
            
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                },

                Buttons = { CloseButton }
            };

            Controller = new Controller("test.kiseki.lol", args);
            Controller.PageHeadingChanged += Controller_PageHeadingChanged;
            Controller.ProgressBarChanged += Controller_ProgressBarChanged;
            Controller.ProgressBarStateChanged += Controller_ProgressBarStateChanged;
            Controller.Launched += Controller_Launched;
            
            Page.Destroyed += (s, e) =>
            {
                Controller.Dispose();
                Environment.Exit(0);
            };

            ShowProgressDialog();
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Controller_PageHeadingChanged(object sender, string Heading)
        {
            Page.Heading = Heading;
        }

        private void Controller_ProgressBarChanged(object sender, int Value)
        {
            Page.ProgressBar!.Value = Value;
        }

        private void Controller_ProgressBarStateChanged(object sender, ProgressBarState State)
        {
            Page.ProgressBar!.State = State switch
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
            Page.Icon = logo;

            Page.Created += (s, e) =>
            {
                Controller.Start();
            };

            TaskDialog.ShowDialog(Page);
        }
    }
}
