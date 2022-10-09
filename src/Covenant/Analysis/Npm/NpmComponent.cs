namespace Covenant.Analysis.Npm;

internal sealed class NpmComponent : BomComponent<NpmVersion>
{
    public NpmComponent(string name, string version, BomComponentKind kind)
        : base(NpmVersion.Parse(version), GetBomRef(name, version, kind), name, version, kind)
    {
    }

    public static string GetBomRef(string name, string version, BomComponentKind kind)
    {
        var prefix = kind == BomComponentKind.Library ? "pkg:npm/" : "pkg:covenant/npm/";
        return $"{prefix}{name}@{version}";
    }
}
