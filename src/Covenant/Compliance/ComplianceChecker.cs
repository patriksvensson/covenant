namespace Covenant.Compliance;

internal sealed class ComplianceChecker
{
    public void PerformComplianceCheck(Bom bom, ComplianceCheckerContext context)
    {
        // Any banned components?
        foreach (var component in bom.Components)
        {
            if (component.License?.Id != null)
            {
                if (context.Configuration.Licenses.Banned.Contains(component.License.Id))
                {
                    context.AddError($"The license '{component.License.Id}' is not allowed")
                        .WithContext("Component", component.Name)
                        .WithContext("Version", component.Version);
                }
            }
        }
    }
}
