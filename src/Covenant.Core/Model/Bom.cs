namespace Covenant.Core.Model;

[DebuggerDisplay("{Name,nq} ({Version,nq})")]
public sealed class Bom
{
    [JsonProperty(Order = 0)]
    public string Name { get; }
    [JsonProperty(Order = 1)]
    public string Version { get; }

    [JsonProperty(Order = 2)]
    public string ToolVendor { get; }
    [JsonProperty(Order = 3)]
    public string ToolVersion { get; }

    [JsonProperty(Order = 4)]
    public List<BomMetadata> Metadata { get; init; }

    [JsonProperty(Order = 5)]
    public List<BomComponent> Components { get; init; }
    [JsonProperty(Order = 6)]
    public List<BomDependency> Dependencies { get; init; }

    public Bom(string name, string version)
    {
        Name = name;
        Version = version;

        ToolVendor = "Covenant";
        ToolVersion = FileVersionInfo.GetVersionInfo(
            typeof(Bom).Assembly.Location).ProductVersion ?? "0.0.0";

        Metadata = new List<BomMetadata>();
        Components = new List<BomComponent>();
        Dependencies = new List<BomDependency>();
    }
}