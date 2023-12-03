namespace Kiseki.Launcher;

public static class Web
{
    public static string? CurrentUrl { get; private set; } = null;
    public static bool IsConnected { get; private set; } = false;
    public static bool IsInMaintenance { get; private set; } = false;

    public static readonly HttpClient HttpClient = new();

    public static void Initialize()
    {
        CurrentUrl = IsInMaintenance ? $"{Constants.MAINTENANCE_DOMAIN}.{Constants.BASE_URL}" : Constants.BASE_URL;

        // Synchronous block is intentional
        Task<HealthCheckStatus> task = GetHealthStatus();
        task.Wait();

        HealthCheckStatus status = task.Result;

        if (status != HealthCheckStatus.Success)
        {
            if (status == HealthCheckStatus.Maintenance)
            {
                IsInMaintenance = true;
            }

            return;
        }

        IsConnected = true;
    }

    public static bool License(string license)
    {
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

    public static async Task<HealthCheckStatus> GetHealthStatus()
    {
        var response = await Http.GetJson<HealthCheck>(FormatUrl("/api/health"));
        
        return response?.Status ?? HealthCheckStatus.Failure;
    }

    public static string FormatUrl(string path, string? subdomain = null, bool forceHttp = false)
    {
        string scheme = "https";
        string url = subdomain == null ? CurrentUrl! : $"{subdomain!}.{CurrentUrl!}";

#if DEBUG
        scheme = "http";
#endif

        if (forceHttp)
        {
            scheme = "http";
        }

        return $"{scheme}://{url}{path}";
    }
}