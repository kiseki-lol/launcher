namespace Kiseki.Launcher.Helpers;

using System.Text.Json;

public static class Http
{
    public static async Task<T?> GetJson<T>(string url)
    {
        try
        {
            string json = await Web.HttpClient.GetStringAsync(url);

            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }
}