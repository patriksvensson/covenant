namespace Covenant;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CovenantLicenseConfiguration
{
    public HashSet<string> Banned { get; }

    public CovenantLicenseConfiguration()
    {
        Banned = new HashSet<string>(StringComparer.Ordinal);
    }
}