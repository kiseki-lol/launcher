using System.Text.Json.Serialization;

namespace Kiseki.Launcher.Models
{
    public class HealthCheck
    {
        [JsonPropertyName("status")]
        public int Status { get; set; } = -1;
    }
}