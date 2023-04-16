namespace Covenant.Middleware;

internal sealed class LicenseDetectorMiddleware : ICovenantMiddleware
{
    public int Order => 1000;

    public Bom Process(MiddlewareContext context, Bom bom)
    {
        foreach (var file in bom.Files)
        {
            Process(file.License);
        }

        foreach (var component in bom.Components)
        {
            Process(component.License);
        }

        return bom;
    }

    private void Process(BomLicense? license)
    {
        if (license == null)
        {
            return;
        }

        if (license.Id != null || license.Expression != null)
        {
            return;
        }

        if (license.Url == null && license.Text == null)
        {
            return;
        }

        if (BomLicenseDetector.TryDetectLicense(
            license.Text?.Decoded,
            license.Text?.Hash,
            license.Url,
            out var id,
            out var expression))
        {
            license.Id = id;
            license.Expression = expression;
        }
    }
}
