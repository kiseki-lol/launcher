using Kiseki.Launcher.Windows.Properties;

namespace Kiseki.Launcher.Windows
{
    [System.ComponentModel.DesignerCategory("")]
    public class MainWindow : Form
    {
        private readonly TaskDialogButton CloseButton;
        private readonly TaskDialogPage Page;
        private readonly Controller Controller;

        public MainWindow(string payload)
        {
            Controller = new Controller(payload);
            Controller.OnPageHeadingChange += Controller_PageHeadingChanged;
            Controller.OnProgressBarAdd += Controller_ProgressBarAdded;
            Controller.OnProgressBarStateChange += Controller_ProgressBarStateChanged;
            Controller.OnErrorShow += Controller_ErrorShown;
            Controller.OnLaunch += (s, e) => Environment.Exit(0);

            CloseButton = TaskDialogButton.Close;
            Page = new TaskDialogPage()
            {
                Caption = Constants.ProjectName,
                AllowMinimize = true,
            
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                },

                Buttons = { CloseButton }
            };

            Page.Created += (s, e) =>
            {
                Controller.Start();
            };
            
            Page.Destroyed += (s, e) =>
            {
                Controller.Dispose();
                Environment.Exit(0);
            };

            ShowProgressDialog();
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            Controller.Dispose();
            Environment.Exit(0);
        }

        private void Controller_PageHeadingChanged(object? sender, string heading)
        {
            Page.Heading = heading;
        }

        private void Controller_ProgressBarAdded(object? sender, int value)
        {
            Page.ProgressBar!.Value += value;
        }

        private void Controller_ProgressBarStateChanged(object? sender, ProgressBarState state)
        {
            Page.ProgressBar!.State = state switch
            {
                ProgressBarState.Normal => TaskDialogProgressBarState.Normal,
                ProgressBarState.Marquee => TaskDialogProgressBarState.Marquee,
                _ => throw new NotImplementedException()
            };
        }

        private void Controller_ErrorShown(object? sender, string[] texts)
        {
            Page.Icon = TaskDialogIcon.Error;
            Page.Heading = texts[0];
            Page.Text = texts[1];

            Controller.Dispose();
        }

        private void ShowProgressDialog()
        {
            TaskDialogIcon logo = new(Resources.IconKiseki);
            Page.Icon = logo;

            TaskDialog.ShowDialog(Page);
        }
    }
}