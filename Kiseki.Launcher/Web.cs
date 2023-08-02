using System.Net;
using System.Text.Json;

namespace Kiseki.Launcher
{
    public static class Web
    {
        public const int RESPONSE_FAILURE = -1;
        public const int RESPONSE_SUCCESS = 0;
        public const int RESPONSE_MAINTENANCE = 1;
        
        public static string CurrentUrl { get; private set; } = "";
        public static bool IsInMaintenance { get; private set; } = false;

        public static readonly HttpClient HttpClient = new();

        public static bool Initialize()
        {
            CurrentUrl = IsInMaintenance ? $"{Constants.MAINTENANCE_DOMAIN}.{Constants.BASE_URL}" : Constants.BASE_URL;

            Task<int> task = CheckHealth();
            task.Wait();

            int response = task.Result;

            if (response != RESPONSE_SUCCESS)
            {
                if (response == RESPONSE_MAINTENANCE)
                    IsInMaintenance = true;

                return false;
            }

            return true;
        }

        public static string Url(string path) => $"https://{CurrentUrl}{path}";

        public static async Task<int> CheckHealth()
        {
            var response = await Helpers.Http.GetJson<Models.HealthCheck>(Url("/api/health"));
            
            return response is null ? RESPONSE_FAILURE : response.Status;
        }

        public static bool LoadLicense(string license)
        {
            // the "license" is actually just headers required to access the website.
            // this can be cloudflare zero-trust headers (like what Kiseki does), or however
            // else you'd like to do auth-walls. either way; it's just a JSON document 

            try
            {
                HttpClient.DefaultRequestHeaders.Clear();
                
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(license)!;

                for (int i = 0; i < headers.Count; i++)
                    HttpClient.DefaultRequestHeaders.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}