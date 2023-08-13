using Covenant.Infrastructure;

namespace Covenant.Core;

public sealed class MiddlewareContext : DiagnosticContext
{
    public string InputPath { get; }
    public CovenantConfiguration Configuration { get; }
    public ICommandLineResolver Cli { get; }

    public MiddlewareContext(
        string inputPath,
        CovenantConfiguration configuration,
        ICommandLineResolver cli)
    {
        InputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Cli = cli ?? throw new ArgumentNullException(nameof(cli));
    }
}