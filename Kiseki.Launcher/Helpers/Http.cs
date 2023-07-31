using System.Text.Json;

namespace Kiseki.Launcher.Helpers
{
    public static class Http
    {
        public static T GetJson<T>(string url)
        {
            try
            {
                string json = Web.HttpClient.GetStringAsync(url).Result;
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }
    }
}