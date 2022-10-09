namespace Covenant.Core.Model;

public sealed class BomLicense
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Expression { get; set; }
    public Base64EncodedText? Text { get; set; }

    [JsonIgnore]
    public bool HasIdentifier => Id != null || Url != null || Name != null || Expression != null;
}