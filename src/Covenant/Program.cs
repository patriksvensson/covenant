using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Covenant.Cli;
using Covenant.Middleware;
using Covenant.Spdx;

namespace Covenant;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var provider = BuildServiceProvider();
        var parser = new CommandLineParser(provider).Create();
        return await parser.InvokeAsync(args);
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IEnvironment, Spectre.IO.Environment>();
        services.AddSingleton<IGlobber, Globber>();
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<ReportGenerator>();

        services.AddSingleton<ICovenantMiddleware, LicenseDetectorMiddleware>();
        services.AddSingleton<BomSerializer, CycloneDxSerializer>();
        services.AddSingleton<BomSerializer, SpdxSerializer>();

        services.AddSingleton(AnsiConsole.Console);

        services.AddSingleton<GenerateCommand>();
        services.AddSingleton<ConvertCommand>();
        services.AddSingleton<ReportCommand>();
        services.AddSingleton<CheckCommand>();

        services.AddSingleton<AnalysisService>();
        services.AddSingleton<Analyzer, DotnetAnalyzer>();
        services.AddSingleton<Analyzer, NpmAnalyzer>();

        services.AddSingleton<CovenantConfigurationReader>();
        services.AddSingleton<ComplianceChecker>();
        services.AddSingleton<DotnetAssetFileReader>();

        return services.BuildServiceProvider();
    }
}
