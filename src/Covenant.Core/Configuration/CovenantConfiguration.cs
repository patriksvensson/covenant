namespace Covenant;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CovenantConfiguration
{
    public CovenantLicenseConfiguration Licenses { get; } = new();
    public List<CovenantFileConfiguration> Files { get; } = new();
    public CovenantExcludeConfiguration Exclude { get; } = new();
}