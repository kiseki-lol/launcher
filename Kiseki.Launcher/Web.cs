namespace Kiseki.Launcher;

using System.Text.Json;

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
        
        // Synchronous block is intentional
        Task<Models.WebResponse> task = CheckHealth();
        task.Wait();

        int response = task.Result.Status;

        if (response != RESPONSE_SUCCESS)
        {
            if (response == RESPONSE_MAINTENANCE) IsInMaintenance = true;

            return false;
        }

        return true;
    }

    public static string Url(string path) => $"https://{CurrentUrl}{path}";

    public static async Task<Models.WebResponse> CheckHealth()
    {
        var response = await Helpers.Http.GetJson<Models.HealthCheck>(Url("/api/health"));
        
        return new Models.WebResponse(response?.Status ?? RESPONSE_FAILURE, response);
    }

    public static bool LoadLicense(string license)
    {
        // The license is just headers required to access the website in a JSON document

        Dictionary<string, string> headers;

        try
        {
            headers = JsonSerializer.Deserialize<Dictionary<string, string>>(license)!;
        }
        catch
        {
            return false;
        }
        
        for (int i = 0; i < headers.Count; i++)
        {
            HttpClient.DefaultRequestHeaders.Add(headers.ElementAt(i).Key, headers.ElementAt(i).Value);
        }

        return true;
    }
}