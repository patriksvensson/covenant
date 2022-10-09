namespace Covenant.Compliance;

internal sealed class ComplianceCheckerContext : DiagnosticContext
{
    public CovenantConfiguration Configuration { get; }

    public ComplianceCheckerContext(CovenantConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
}
