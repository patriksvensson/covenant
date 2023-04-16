namespace Covenant.Reporting;

public class ReportLicenseInformation
{
    /// <summary>
    /// Gets all known SPDX license identifiers.
    /// </summary>
    public LicenseCollection Known { get; }

    /// <summary>
    /// Gets all unknown SPDX license identifiers.
    /// </summary>
    public LicenseCollection Unknown { get; }

    /// <summary>
    /// Gets all known SPDX license identifiers.
    /// </summary>
    public LicenseCollection Embedded { get; }

    /// <summary>
    /// Gets the count of total unknown licenses.
    /// </summary>
    public LicenseCount TotalUnknown =>
        new LicenseCount(
            Unknown.Count + Embedded.Count,
            Unknown.Distinct + Embedded.Distinct);

    /// <summary>
    /// Gets the number of unlicensed items.
    /// </summary>
    public int UnlicensedCount { get; private set; }

    public ReportLicenseInformation()
    {
        Known = new LicenseCollection();
        Unknown = new LicenseCollection();
        Embedded = new LicenseCollection();
    }

    public static ReportLicenseInformation Create(Bom bom)
    {
        var info = new ReportLicenseInformation();

        foreach (var component in bom.Files)
        {
            info.AddFile(component);
        }

        foreach (var component in bom.Components)
        {
            info.AddComponent(component);
        }

        return info;
    }

    public void AddFile(BomFile file)
    {
        if (file.License == null)
        {
            UnlicensedCount++;
        }

        TryAddLicense(file.License);
    }

    public void AddComponent(BomComponent component)
    {
        // Unlicensed?
        if (!component.IsRoot && component.License == null)
        {
            UnlicensedCount++;
        }

        TryAddLicense(component.License);
    }

    private void TryAddLicense(BomLicense? license)
    {
        if (license == null)
        {
            return;
        }

        if (license?.Id != null ||
            license?.Expression != null)
        {
            // Got ID or expression
            var licenseId = license.Id ?? license.Expression;
            if (licenseId == null)
            {
                throw new InvalidOperationException("Invalid license");
            }

            Known.Add(licenseId);
        }
        else if (license?.Id == null &&
                 license?.Expression == null &&
                 license?.Text == null)
        {
            // Nothing to identify the license on
            var licenseId = license?.Name ?? license?.Url;
            if (licenseId == null)
            {
                throw new InvalidOperationException("Invalid license");
            }

            Unknown.Add(licenseId);
        }
        else if (license.Text != null)
        {
            // Embedded text
            Embedded.Add(license.Text.Decoded);
        }
    }
}
