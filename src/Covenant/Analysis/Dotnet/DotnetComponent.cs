namespace Covenant.Analysis.Dotnet;

internal sealed class DotnetComponent : BomComponent<NuGetVersion>
{
    public DotnetComponent(string name, NuGetVersion version, BomComponentKind kind = BomComponentKind.Library)
        : base(version, GetBomRef(name, version, kind), name, version.OriginalVersion, kind)
    {
    }

    public static string GetBomRef(string name, NuGetVersion version, BomComponentKind kind = BomComponentKind.Library)
    {
        var prefix = kind == BomComponentKind.Library ? "pkg:nuget/" : "pkg:covenant/dotnet/";
        return $"{prefix}{name}@{version.OriginalVersion}";
    }
}
