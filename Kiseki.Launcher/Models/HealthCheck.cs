namespace Kiseki.Launcher.Models;

public class HealthCheck
{
    [JsonPropertyName("status")]
    public HealthCheckStatus Status { get; set; }
}