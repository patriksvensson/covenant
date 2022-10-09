namespace Covenant.Core.Model;

public sealed class BomComponentComparer : IEqualityComparer<BomComponent>
{
    public static BomComponentComparer Shared { get; } = new BomComponentComparer();

    public bool Equals(BomComponent? x, BomComponent? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Purl?.Equals(y.Purl, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public int GetHashCode([DisallowNull] BomComponent obj)
    {
        return obj?.Purl?.GetHashCode() ?? 0;
    }
}