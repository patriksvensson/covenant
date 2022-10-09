namespace Covenant.Analysis.Npm;

internal sealed class PackageLockJsonPackage
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("optional")]
    public bool? Optional { get; set; }

    [JsonProperty("integrity")]
    public string? Integrity { get; set; }

    [JsonProperty("resolved")]
    public string? Resolved { get; set; }

    [JsonProperty("requires")]
    public Dictionary<string, string>? Requires { get; set; }
}
