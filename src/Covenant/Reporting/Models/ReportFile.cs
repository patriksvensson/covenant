namespace Covenant.Reporting;

public sealed class ReportFile
{
    private readonly BomFile _file;

    public string Path => _file.Path;
    public BomLicense? License => _file.License;

    public ReportFile(BomFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }
}
