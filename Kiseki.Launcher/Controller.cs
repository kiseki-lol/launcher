using Kiseki.Launcher.Helpers;

namespace Kiseki.Launcher
{
    public enum ProgressBarState
    {
        Normal,
        Marquee
    }

    public class Controller
    {
        private readonly Dictionary<string, string> Arguments = new();

        public event EventHandler<string>? OnPageHeadingChange;
        public event EventHandler<int>? OnProgressBarAdd;
        public event EventHandler<ProgressBarState>? OnProgressBarStateChange;
        public event EventHandler<string[]>? OnErrorShow;
        public event EventHandler? OnLaunch;

        public Controller(string payload)
        {
            if (!Base64.IsBase64String(payload))
            {
                ErrorShow($"Failed to launch {Constants.ProjectName}", $"Try launching {Constants.ProjectName} from the website again.");
                return;
            }

            // TODO: The payload will soon include more members; update this accordingly
            payload = Base64.ConvertBase64ToString(payload);
            if (payload.Split("|").Length != 2)
            {
                ErrorShow($"Failed to launch {Constants.ProjectName}", $"Try launching {Constants.ProjectName} from the website again.");
                return;
            }

            Arguments["JoinScriptURL"] = payload.Split("|")[0];
            Arguments["Ticket"] = payload.Split("|")[1];
        }
        
        public async void Start()
        {
            PageHeadingChange("Connecting to Kiseki...");

            bool marquee = true;
            await foreach (int progressValue in StreamBackgroundOperationProgressAsync())
            {
                if (marquee)
                {
                    PageHeadingChange("Downloading Kiseki...");
                    ProgressBarStateChange(ProgressBarState.Normal);
                    marquee = false;
                }

                ProgressBarAdd(progressValue);
            }

            static async IAsyncEnumerable<int> StreamBackgroundOperationProgressAsync()
            {
                await Task.Delay(2800);

                yield return 4;
                await Task.Delay(200);
            }

            PageHeadingChange("Installing Kiseki...");
            ProgressBarStateChange(ProgressBarState.Marquee);

            await Task.Delay(2200);
            PageHeadingChange("Configuring Kiseki...");

            await Task.Delay(1200);
            PageHeadingChange("Launching Kiseki...");

            await Task.Delay(3000);
            Launch();
        }

        public async void Dispose()
        {
            // TODO: This will only be called when the user closes the window OR we're done (i.e. the Launched event is called.)
        }

        protected virtual void PageHeadingChange(string heading)
        {
            OnPageHeadingChange!.Invoke(this, heading);
        }

        protected virtual void ProgressBarAdd(int value)
        {
            OnProgressBarAdd!.Invoke(this, value);
        }

        protected virtual void ProgressBarStateChange(ProgressBarState state)
        {
            OnProgressBarStateChange!.Invoke(this, state);
        }

        protected virtual void ErrorShow(string heading, string text)
        {
            // ugly hack for now (I don't want to derive EventHandler just for this)
            OnErrorShow!.Invoke(this, new string[] { heading, text });
        }

        protected virtual void Launch()
        {
            OnLaunch!.Invoke(this, EventArgs.Empty);
        }
    }
}