namespace Kiseki.Launcher.Models;

using System.Text.Json.Serialization;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("body")]
    public string Body { get; set; } = null!;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = null!;

    [JsonPropertyName("assets")]
    public List<GitHubReleaseAsset>? Assets { get; set; }
}

public class GitHubReleaseAsset
{
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}