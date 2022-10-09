namespace Covenant.Spdx.Model;

internal sealed class CovenantSpdxPackage : SpdxPackage
{
    public BomComponent Component { get; }

    public CovenantSpdxPackage(BomComponent component)
    {
        Component = component ?? throw new ArgumentNullException(nameof(component));
    }
}