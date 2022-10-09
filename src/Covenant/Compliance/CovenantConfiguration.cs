namespace Covenant;

internal sealed class CovenantConfiguration
{
    public CovenantLicenseConfiguration Licenses { get; }

    public CovenantConfiguration()
    {
        Licenses = new CovenantLicenseConfiguration();
    }
}
