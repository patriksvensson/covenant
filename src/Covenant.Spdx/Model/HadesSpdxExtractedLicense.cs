namespace Covenant.Spdx.Model;

internal sealed class CovenantSpdxExtractedLicense : SpdxExtractedLicense
{
    public BomLicense License { get; }

    public CovenantSpdxExtractedLicense(BomLicense license)
    {
        License = license ?? throw new ArgumentNullException(nameof(license));
    }
}