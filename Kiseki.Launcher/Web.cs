using System.Net;
using System.Text.Json;

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
        public static string Url(string path) => $"https://{CurrentUrl}{path}";

        public static int CheckHealth()
        {
            var response = Helpers.Http.GetJson<Models.HealthCheck>(Url("/api/health"));
            
            return response is null ? RESPONSE_FAILURE : response.Status;
        }

        public static bool LoadLicense(string license)
        {
            CurrentUrl = $"{MaintenanceDomain}.{CurrentUrl}";

            // the "license" is actually just headers required to access the website.
            // this can be cloudflare zero-trust headers (like what Kiseki does), or however
            // else you'd like to do auth-walls. either way; it's just a JSON document that
            // has each byte XORed by 55 (for some basic obfuscation).

            try
            {
                string json = "";
                for (int i = 0; i < license.Length; i++)
                {
                    json += (char)(license[i] ^ 55);
                }

                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
                for (int i = 0; i < headers.Count; i++)
                {
                    HttpClient.DefaultRequestHeaders.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
                }
            }
            catch
            {
                MessageBox.Show($"Corrupt license file! Please verify the contents of your license file (it should be named \"license.bin\".)", Constants.ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}