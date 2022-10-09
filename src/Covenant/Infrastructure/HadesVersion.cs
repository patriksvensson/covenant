namespace Covenant.Infrastructure;

public static class CovenantVersion
{
    public static string Version { get; }

    static CovenantVersion()
    {
        Version = FileVersionInfo.GetVersionInfo(
            typeof(Bom).Assembly.Location)
                .ProductVersion ?? "0.0.0";
    }
}