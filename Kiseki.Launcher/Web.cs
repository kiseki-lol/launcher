using System.Net;
using System.Text.Json;

namespace Kiseki.Launcher
{
    public static class Web
    {
        public const int RESPONSE_FAILURE = -1;
        public const int RESPONSE_SUCCESS = 0;
        public const int RESPONSE_MAINTENANCE = 1;
        
        public const string BASE_URL = "kiseki.lol";
        public const string MAINTENANCE_DOMAIN = "test";
        
        public static readonly HttpClient HttpClient = new();
        public static string CurrentUrl { get; private set; } = "";
        public static bool MaintenanceMode { get; private set; } = false;

        public static void Initialize() => CurrentUrl = BASE_URL;
        public static string Url(string path) => $"https://{CurrentUrl}{path}";

        public static int CheckHealth()
        {
            var response = Helpers.Http.GetJson<Models.HealthCheck>(Url("/api/health"));
            
            return response is null ? RESPONSE_FAILURE : response.Status;
        }

        public static bool LoadLicense(string license)
        {
            if (!MaintenanceMode)
            {
                CurrentUrl = $"{MAINTENANCE_DOMAIN}.{CurrentUrl}";
                MaintenanceMode = true;
            }

            // the "license" is actually just headers required to access the website.
            // this can be cloudflare zero-trust headers (like what Kiseki does), or however
            // else you'd like to do auth-walls. either way; it's just a JSON document 

            try
            {
                HttpClient.DefaultRequestHeaders.Clear();
                
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(license)!;
                for (int i = 0; i < headers.Count; i++)
                {
                    HttpClient.DefaultRequestHeaders.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}