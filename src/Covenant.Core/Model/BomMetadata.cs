namespace Covenant.Core.Model;

public sealed class BomMetadata
{
    public string Key { get; }
    public string Value { get; }

    public BomMetadata(string key, string value)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}