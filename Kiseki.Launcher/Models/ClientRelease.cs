namespace Kiseki.Launcher.Models;

using System.Text.Json.Serialization;

public class ClientRelease
{
    [JsonPropertyName("checksums")]
    public Dictionary<string, string> Checksums { get; set; } = null!;

    [JsonPropertyName("asset")]
    public ClientReleaseAsset Asset { get; set; } = null!;
}

public class ClientReleaseAsset
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; } = null!;
}