namespace Covenant;

internal sealed class CovenantLicenseConfiguration
{
    public HashSet<string> Banned { get; }

    public CovenantLicenseConfiguration()
    {
        Banned = new HashSet<string>(StringComparer.Ordinal);
    }
}
