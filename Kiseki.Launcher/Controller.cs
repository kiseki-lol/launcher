namespace Kiseki.Launcher
{
    public enum ProgressBarState
    {
        Normal,
        Marquee
    }

    public class Controller
    {
        private readonly string BaseURL;
        private readonly string JoinScriptURL;
        private readonly string Ticket;

        public event EventHandler<string>? PageHeadingChanged;
        public event EventHandler<int>? ProgressBarChanged;
        public event EventHandler<ProgressBarState>? ProgressBarStateChanged;
        public event EventHandler? Launched;

        public Controller(string BaseURL, string[] Arguments)
        {
            this.BaseURL = BaseURL;

            // TODO: handle these more gracefully
            if (Arguments.Length != 0 || Arguments[0] != "launch")
            {
                Environment.Exit(0);
            }

            if (!Helpers.IsBase64String(Arguments[0]))
            {
                Environment.Exit(0);
            }

            string payload = Helpers.ConvertBase64ToString(Arguments[0]);
            if (payload.Split("|").Length != 2) // joinscripturl, ticket; TODO: this will also include launchmode (ide/play)
            {
                Environment.Exit(0);
            }

            JoinScriptURL = payload.Split("|")[0];
            Ticket = payload.Split("|")[1];
        }
        
        public async void Start()
        {
            OnPageHeadingChange("Connecting to Kiseki...");

            bool marquee = true;
            await foreach (int progressValue in StreamBackgroundOperationProgressAsync())
            {
                if (marquee)
                {
                    OnPageHeadingChange("Downloading Kiseki...");
                    OnProgressBarStateChanged(ProgressBarState.Normal);
                    marquee = false;
                }

                OnProgressBarChange(progressValue);
            }

            static async IAsyncEnumerable<int> StreamBackgroundOperationProgressAsync()
            {
                await Task.Delay(2800);

                for (int i = 0; i <= 100; i += 4)
                {
                    yield return i;
                    await Task.Delay(200);
                }
            }

            OnPageHeadingChange("Installing Kiseki...");
            OnProgressBarStateChanged(ProgressBarState.Marquee);

            await Task.Delay(2200);
            OnPageHeadingChange("Configuring Kiseki...");

            await Task.Delay(1200);
            OnPageHeadingChange("Launching Kiseki...");

            await Task.Delay(3000);
            OnLaunched();
        }

        public async void Dispose()
        {
            // TODO: This will only be called when the user closes the window OR we're done (i.e. the Launched event is called.)
        }

        protected virtual void OnPageHeadingChange(string Heading)
        {
            PageHeadingChanged!.Invoke(this, Heading);
        }

        protected virtual void OnProgressBarChange(int Value)
        {
            ProgressBarChanged!.Invoke(this, Value);
        }

        protected virtual void OnProgressBarStateChanged(ProgressBarState State)
        {
            ProgressBarStateChanged!.Invoke(this, State);
        }

        protected virtual void OnLaunched()
        {
            Launched!.Invoke(this, EventArgs.Empty);
        }
    }
}