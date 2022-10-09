namespace Covenant.Core.Model;

[DebuggerDisplay("{Purl,nq}")]
public sealed class BomDependency
{
    public string Purl { get; }
    public List<string> Dependencies { get; set; }

    public BomDependency(string purl)
    {
        Purl = purl ?? throw new ArgumentNullException(nameof(purl));
        Dependencies = new List<string>();
    }
}