namespace Covenant.Spdx.Model;

internal sealed class CovenantSpdxDocument
    : SpdxDocument<CovenantSpdxPackage, SpdxFile, SpdxRelationship, CovenantSpdxExtractedLicense>
{
    public CovenantSpdxPackage? FindPackageByPurl(string @ref)
    {
        foreach (var package in Packages)
        {
            if (package.Component.Purl.Equals(@ref))
            {
                return package;
            }
        }

        return null;
    }

    public CovenantSpdxExtractedLicense? AddLicense(BomLicense license)
    {
        if (license.Id != null && SpdxExpression.TryParse(license.Id, SpdxLicenseOptions.Strict, out _))
        {
            return null;
        }

        if (license.Expression != null && SpdxExpression.TryParse(license.Expression, SpdxLicenseOptions.Strict, out _))
        {
            return null;
        }

        if (FindLicense(license) != null)
        {
            return null;
        }

        var result = new CovenantSpdxExtractedLicense(license)
        {
            LicenseName = license.Name!,
            LicenseComment = license.Id!,
            ExtractedText = license.Text?.Decoded ?? "NONE",
        };

        if (license.Url != null)
        {
            result.LicenseCrossReference = new List<string>
            {
                license.Url,
            };
        }

        ExtractedLicenses.Add(result);

        return result;
    }

    public CovenantSpdxExtractedLicense? FindLicense(BomLicense? license)
    {
        if (license == null)
        {
            return null;
        }

        foreach (var extractedLicense in ExtractedLicenses)
        {
            if (extractedLicense.License.Id == license.Id &&
                extractedLicense.License.Expression == license.Expression &&
                extractedLicense.License.Text?.Decoded == license.Text?.Decoded &&
                extractedLicense.License.Url == license.Url)
            {
                return extractedLicense;
            }
        }

        return null;
    }
}