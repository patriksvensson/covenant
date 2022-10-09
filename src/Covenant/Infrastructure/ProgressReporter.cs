namespace Covenant.Infrastructure;

internal sealed class ProgressReporter : IProgress<string>
{
    private readonly StatusContext _ctx;

    public ProgressReporter(StatusContext ctx)
    {
        _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
    }

    public void Report(string value)
    {
        _ctx.Status(value);
    }
}
