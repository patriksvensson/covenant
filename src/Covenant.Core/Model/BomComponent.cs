namespace Covenant.Core.Model;

[DebuggerDisplay("{Name,nq} {Version,nq}")]
public class BomComponent
{
    public Guid UUID { get; }

    public string Purl { get; }
    public string Name { get; }
    public string Version { get; }
    public BomComponentKind Kind { get; }

    public string? Copyright { get; set; }
    public BomHash? Hash { get; set; }
    public BomLicense? License { get; set; }
    public string? Source { get; set; }
    public HashSet<string> Groups { get; }

    public bool IsRoot => Kind == BomComponentKind.Root;

    public BomComponent(string purl, string name, string version, BomComponentKind kind)
    {
        UUID = Guid.NewGuid();
        Purl = purl ?? throw new ArgumentNullException(nameof(purl));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Kind = kind;
        Groups = new HashSet<string>(StringComparer.Ordinal);
    }

    public BomComponent AddParent(string? parent)
    {
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Groups.Add(parent);
        }

        return this;
    }

    public BomComponent SetLicense(BomLicense? license)
    {
        License = license;
        return this;
    }

    public BomComponent SetCopyright(string? copyright)
    {
        Copyright = copyright;
        return this;
    }

    public BomComponent SetHash(BomHash? hash)
    {
        Hash = hash;
        return this;
    }
}

[DebuggerDisplay("{Name,nq} {Version,nq} ({Source,nq})")]
public class BomComponent<T> : BomComponent
    where T : class
{
    public T Data { get; }

    public BomComponent(T metadata, string @ref, string name, string version, BomComponentKind kind)
        : base(@ref, name, version, kind)
    {
        Data = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }
}