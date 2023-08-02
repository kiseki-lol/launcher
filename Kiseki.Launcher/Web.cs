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

        public static bool Initialize()
        {
            CurrentUrl = BASE_URL;

            int response = CheckHealth();
            
            if (response != RESPONSE_SUCCESS)
            {
                if (response == RESPONSE_MAINTENANCE)
                {
                    CurrentUrl = $"{MAINTENANCE_DOMAIN}.{BASE_URL}";
                    return Initialize();
                }

                return false;
            }

            return true;
        }

        public static string Url(string path) => $"https://{CurrentUrl}{path}";

        public static int CheckHealth()
        {
            var response = Helpers.Http.GetJson<Models.HealthCheck>(Url("/api/health"));
            
            return response is null ? RESPONSE_FAILURE : response.Status;
        }
    }
}