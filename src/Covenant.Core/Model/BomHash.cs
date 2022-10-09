namespace Covenant.Core.Model;

public sealed class BomHash
{
    public BomHashAlgorithm Algorithm { get; }
    public string Content { get; }

    public BomHash(BomHashAlgorithm algorithm, string content)
    {
        Algorithm = algorithm;
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}