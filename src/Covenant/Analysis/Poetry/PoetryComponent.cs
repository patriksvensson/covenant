namespace Covenant.Analysis.Poetry;

internal sealed class PoetryComponent : BomComponent<PoetryVersion>
{
    public PoetryComponent(string name, string version, BomComponentKind kind)
        : base(PoetryVersion.Parse(version), GetBomRef(name, version, kind), name, version, kind)
    {
    }

    public static string GetBomRef(string name, string version, BomComponentKind kind)
    {
        var prefix = kind == BomComponentKind.Library ? "pkg:poetry/" : "pkg:covenant/poetry/";
        return $"{prefix}{name}@{version}";
    }
}
