namespace Kiseki.Launcher
{
    public static class Web
    {
        public const int RESPONSE_SUCCESS = 0;
        public const int RESPONSE_FAILURE = -1;

        public const int RESPONSE_MAINTENANCE = 1;
        
        public const string BaseUrl = "kiseki.lol";
        public const string MaintenanceDomain = "test";
        
        public static readonly HttpClient HttpClient = new();
        public static string CurrentUrl { get; private set; } = "";

        public static void Initialize() => CurrentUrl = BaseUrl;
        public static string Url(string path) => $"https://{CurrentUrl}/{path}";

        public static async Task<int> CheckHealth()
        {
            var response = await Helpers.Http.GetJson<Models.Health>(Url("/api/health"));

            return response?.Response ?? RESPONSE_FAILURE;
        }

        public static void LoadLicense(string license)
        {
            CurrentUrl = $"{MaintenanceDomain}.{CurrentUrl}";
            

        }
    }
}