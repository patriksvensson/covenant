namespace Covenant.Analysis.Npm;

internal sealed class PackageJson
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("license")]
    public object? License { get; set; }

    [JsonProperty("dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; }

    [JsonProperty("devDependencies")]
    public Dictionary<string, string>? DevDependencies { get; set; }
}
