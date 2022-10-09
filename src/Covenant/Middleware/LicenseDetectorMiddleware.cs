namespace Covenant.Middleware;

internal sealed class LicenseDetectorMiddleware : ICovenantMiddleware
{
    public Bom Process(Bom bom, ICommandLineResolver cli)
    {
        foreach (var component in bom.Components)
        {
            if (component.License == null)
            {
                continue;
            }

            if (component.License.Id != null || component.License.Expression != null)
            {
                continue;
            }

            if (component.License.Url == null && component.License.Text == null)
            {
                continue;
            }

            if (BomLicenseDetector.TryDetectLicense(
                component.License.Text?.Decoded,
                component.License.Text?.Hash,
                component.License.Url,
                out var id,
                out var expression))
            {
                component.License.Id = id;
                component.License.Expression = expression;
            }
        }

        return bom;
    }
}
