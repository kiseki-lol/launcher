using Kiseki.Launcher.Windows.Properties;

namespace Kiseki.Launcher.Windows;

[System.ComponentModel.DesignerCategory("")]
public class MainWindow : Form
{
    private readonly TaskDialogButton CloseButton;
    private readonly TaskDialogPage Page;
    private readonly Bootstrapper Bootstrapper;

    public MainWindow(string payload)
    {
        Bootstrapper = new Bootstrapper(payload);
        Bootstrapper.OnHeadingChange += Bootstrapper_HeadingChanged;
        Bootstrapper.OnProgressBarSet += Bootstrapper_ProgressBarSet;
        Bootstrapper.OnProgressBarStateChange += Bootstrapper_ProgressBarStateChanged;
        Bootstrapper.OnError += Bootstrapper_Errored;

        CloseButton = TaskDialogButton.Close;
        Page = new TaskDialogPage()
        {
            Caption = Constants.PROJECT_NAME,
            AllowMinimize = true,
        
            ProgressBar = new TaskDialogProgressBar()
            {
                State = TaskDialogProgressBarState.Marquee
            },

            Buttons = { CloseButton }
        };

        Page.Created += (_, _) =>
        {
            if (!Bootstrapper.Initialize())
            {
                Page.Heading = $"Failed to launch {Constants.PROJECT_NAME}";
                Page.Text = $"Try launching {Constants.PROJECT_NAME} from the website again.";
                Page.ProgressBar!.State = TaskDialogProgressBarState.Error;

                return;
            }
            
            Bootstrapper.Run();
        };

        // This is a small hack to ensure that the underlying Form that MainWindow has
        // doesn't open when the TaskDialogPage is destroyed.
        Page.Destroyed += (_, _) =>
        {
            Environment.Exit(0);
        };

        ShowTaskDialog();
    }

    private void ShowTaskDialog()
    {
        TaskDialogIcon logo = new(Resources.IconKiseki);
        Page.Icon = logo;

        TaskDialog.ShowDialog(Page);
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    private void Bootstrapper_HeadingChanged(object? sender, string heading)
    {
        Page.Heading = heading;
    }

    private void Bootstrapper_ProgressBarSet(object? sender, int value)
    {
        Page.ProgressBar!.Value = value;
    }

    private void Bootstrapper_ProgressBarStateChanged(object? sender, Enums.ProgressBarState state)
    {
        Page.ProgressBar!.State = state switch
        {
            Enums.ProgressBarState.Normal => TaskDialogProgressBarState.Normal,
            Enums.ProgressBarState.Marquee => TaskDialogProgressBarState.Marquee,
            _ => throw new NotImplementedException()
        };
    }

    private void Bootstrapper_Errored(object? sender, string[] texts)
    {
        Page.Heading = texts[0];
        Page.Text = texts[1];
        Page.ProgressBar!.State = TaskDialogProgressBarState.Error;
    }
}