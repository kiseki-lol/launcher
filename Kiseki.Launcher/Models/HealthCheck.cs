namespace Kiseki.Launcher.Models;

using System.Text.Json.Serialization;

public class HealthCheck
{
    [JsonPropertyName("status")]
    public int Status { get; set; } = -1;
}