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
        private readonly string BaseURL;
        private readonly Dictionary<string, string> Arguments = new();

        public event EventHandler<string>? OnPageHeadingChanged;
        public event EventHandler<int>? OnProgressBarChanged;
        public event EventHandler<ProgressBarState>? OnProgressBarStateChanged;
        public event EventHandler? OnLaunched;

        public static readonly HttpClient HttpClient = new();

        public Controller(string baseURL, string[] args)
        {
            BaseURL = baseURL;

            if (args.Length > 0)
            {
                // TODO: handle these more gracefully
                if (!Base64.IsBase64String(args[0]))
                {
                    Environment.Exit(0);
                }

                // TODO: the payload will soon include more members, such as whether to open the IDE or not (as well as values required for our weird loopback authentication thing)
                string payload = Base64.ConvertBase64ToString(args[0]);
                if (payload.Split("|").Length != 2)
                {
                    Environment.Exit(0);
                }

                Arguments["JoinScriptURL"] = payload.Split("|")[0];
                Arguments["Ticket"] = payload.Split("|")[1];
            }
        }
        
        public async void Start()
        {
            PageHeadingChange("Connecting to Kiseki...");
            
            var response = await Http.GetJson<Models.Health>($"https://{BaseURL}/api/health");
            if (response is null)
            {
                PageHeadingChange("Failed to connect");
                return;
            }

            bool marquee = true;
            await foreach (int progressValue in StreamBackgroundOperationProgressAsync())
            {
                if (marquee)
                {
                    PageHeadingChange("Downloading Kiseki...");
                    ProgressBarStateChanged(ProgressBarState.Normal);
                    marquee = false;
                }

                ProgressBarChange(progressValue);
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

            PageHeadingChange("Installing Kiseki...");
            ProgressBarStateChanged(ProgressBarState.Marquee);

            await Task.Delay(2200);
            PageHeadingChange("Configuring Kiseki...");

            await Task.Delay(1200);
            PageHeadingChange("Launching Kiseki...");

            await Task.Delay(3000);
            Launched();
        }

        public async void Dispose()
        {
            // TODO: This will only be called when the user closes the window OR we're done (i.e. the Launched event is called.)
        }

        protected virtual void PageHeadingChange(string Heading)
        {
            OnPageHeadingChanged!.Invoke(this, Heading);
        }

        protected virtual void ProgressBarChange(int Value)
        {
            OnProgressBarChanged!.Invoke(this, Value);
        }

        protected virtual void ProgressBarStateChanged(ProgressBarState State)
        {
            OnProgressBarStateChanged!.Invoke(this, State);
        }

        protected virtual void Launched()
        {
            OnLaunched!.Invoke(this, EventArgs.Empty);
        }
    }
}