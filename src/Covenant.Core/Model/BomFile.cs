namespace Covenant.Core.Model;

public sealed class BomFile
{
    public string Path { get; }
    public BomLicense? License { get; set; }
    public BomHash Hash { get; set; }

    public BomFile(string path, BomHash hash)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));
    }
}