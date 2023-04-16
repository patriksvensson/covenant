namespace Covenant;

public sealed class CovenantConfiguration
{
    public CovenantLicenseConfiguration Licenses { get; }
    public List<CovenantFileConfiguration> Files { get; }

    public CovenantConfiguration()
    {
        Licenses = new CovenantLicenseConfiguration();
        Files = new List<CovenantFileConfiguration>();
    }
}