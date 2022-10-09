namespace Covenant.Analysis.Npm;

internal sealed class PackageLockJson
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("lockFileVersion")]
    public int LockFileVersion { get; set; }

    [JsonProperty("packages")]
    public Dictionary<string, PackageLockJsonPackage>? Packages { get; set; }

    [JsonProperty("dependencies")]
    public Dictionary<string, PackageLockJsonPackage>? Dependencies { get; set; }
}
