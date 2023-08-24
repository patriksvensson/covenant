using System.Security.Cryptography;
using Buildalyzer;
using Microsoft.Build.Construction;

namespace Covenant.Analysis.Dotnet;

internal class DotnetAnalyzer : Analyzer
{
    private const string DesignTimeBuildFlag = "--design-time-build";
    private const string DisableDotnet = "--disable-dotnet";

    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly IGlobber _globber;
    private readonly NuspecLicenseReader _nuspecReader;
    private readonly DotnetAssetFileReader _assetFileReader;
    private readonly AnalyzerManager _analyzerManager;
    private bool _enabled = true;

    public override bool Enabled => _enabled;
    public override string[] Patterns { get; } = new[] { "**/*.sln" };

    public DotnetAnalyzer(
        IFileSystem fileSystem, IEnvironment environment, IGlobber globber)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _globber = globber ?? throw new ArgumentNullException(nameof(globber));
        _nuspecReader = new NuspecLicenseReader(_fileSystem);
        _assetFileReader = new DotnetAssetFileReader(_fileSystem);
        _analyzerManager = new AnalyzerManager();
    }

    public override void Initialize(ICommandLineAugmentor cli)
    {
        cli.AddOption<bool>(DesignTimeBuildFlag, "Performs a design time build for .NET projects", false);
    }

    public override void BeforeAnalysis(AnalysisSettings settings)
    {
        if (settings.Cli.GetOption<bool>(DisableDotnet))
        {
            _enabled = false;
        }
    }

    public override bool ShouldTraverse(DirectoryPath path)
    {
        return !path.Segments.Any(s => s.Equals("bin"))
            && !path.Segments.Any(s => s.Equals("obj"));
    }

    public override bool CanHandle(AnalysisContext context, FilePath path)
    {
        var isSolution = path.GetExtension()?.Equals(".sln", StringComparison.OrdinalIgnoreCase) ?? false;
        var isProject = path.GetExtension()?.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ?? false;
        return isSolution || isProject;
    }

    public override void Analyze(AnalysisContext context, FilePath path)
    {
        path = path.MakeAbsolute(_environment);
        var extension = path.GetExtension();

        if (extension?.Equals(".sln", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            // Analyze solution
            var solution = SolutionFile.Parse(path.FullPath);

            // Collect all projects
            var projects = solution.ProjectsInOrder.Where(
                csproj => csproj.ProjectType != SolutionProjectType.SolutionFolder
                    && csproj.ProjectType != SolutionProjectType.Unknown);

            // Add all components in all projects
            var assetFiles = new List<AssetFile>();
            foreach (var csproj in projects)
            {
                var assetsFile = ReadAssetFile(context, new FilePath(csproj.AbsolutePath));
                if (assetsFile != null)
                {
                    assetFiles.Add(assetsFile);
                    AnalyzeProject(context, assetsFile, path.GetFilename().FullPath);
                }
            }

            // Add all dependencies for all components in all projects
            foreach (var assetFile in assetFiles)
            {
                AnalyzeProjectDependencies(context, assetFile);
            }
        }
        else if (extension?.Equals(".csproj", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            var assetsFile = ReadAssetFile(context, path);
            if (assetsFile != null)
            {
                AnalyzeProject(context, assetsFile);
                AnalyzeProjectDependencies(context, assetsFile);
            }
        }
    }

    private void AnalyzeProject(AnalysisContext context, AssetFile assets, string? group = null)
    {
        var (version, copyright) = PerformDesignTimeBuild(context, assets);

        // Add project as a component
        context
            .AddComponent(
                new DotnetComponent(
                    assets.Project.Restore.Name,
                    version ?? assets.Project.Version,
                    BomComponentKind.Root))
            .SetCopyright(copyright)
            .AddParent(group);

        // Add all libraries
        foreach (var library in assets.Libraries)
        {
            if (library.Type == LibraryType.Project)
            {
                context
                    .AddComponent(
                        new DotnetComponent(
                            library.Name,
                            library.Version,
                            BomComponentKind.Root))
                    .AddParent(group);
            }
            else
            {
                var license = default(BomLicense);
                var hash = default(BomHash);
                var packageCopyright = default(string);

                var packagePath = GetNuGetPackagePath(context, assets, library);
                if (packagePath != null)
                {
                    // Calculate the hash
                    using (var sha = SHA512.Create())
                    {
                        using (var stream = _fileSystem.GetFile(packagePath.FullPath).OpenRead())
                        {
                            hash = new BomHash(
                                BomHashAlgorithm.SHA512,
                                BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty));
                        }
                    }

                    // Read the license
                    if (_nuspecReader.TryReadLicense(packagePath, out var licenseInfo))
                    {
                        packageCopyright = licenseInfo.Copyright;
                        license = licenseInfo.ConvertToBomLicense();
                    }
                    else
                    {
                        context.AddError($"Could not parse the NuGet package information '{packagePath.FullPath}'");
                    }
                }

                context
                    .AddComponent(
                        new DotnetComponent(
                            library.Name,
                            library.Version))
                    .SetLicense(license)
                    .SetCopyright(packageCopyright)
                    .SetHash(hash);
            }
        }
    }

    private static bool AnalyzeProjectDependencies(AnalysisContext context, AssetFile assets)
    {
        foreach (var target in assets.Targets)
        {
            using (context.Scope(target.TargetFramework))
            {
                foreach (var library in target.Libraries)
                {
                    var targetLibraryRef = DotnetComponent.GetBomRef(
                        library.Name,
                        library.Version,
                        library.Type == LibraryType.Project ? BomComponentKind.Root : BomComponentKind.Library);

                    var targetLibraryComponent = context.Graph.FindByBomRef(targetLibraryRef);
                    if (targetLibraryComponent == null)
                    {
                        context.AddError($"Could not find library '{library.Name}' in graph");
                        return false;
                    }

                    foreach (var dependency in library.Dependencies)
                    {
                        // Find the best suitable version
                        var dependencyComponent = context.Graph.FindDotnetComponent(dependency.Id, dependency.Version);
                        if (dependencyComponent == null)
                        {
                            context.AddWarning(
                                $"The dependency [yellow]{dependency.Id}[/]@[yellow]{dependency.Version.OriginalString.EscapeMarkup()}[/] " +
                                $"for [green]{targetLibraryComponent.Name}[/]@[green]{targetLibraryComponent.Version.EscapeMarkup()}[/] was not found.");

                            continue;
                        }

                        // Connect
                        context.Connect(targetLibraryComponent, dependencyComponent, target.TargetFramework);
                    }
                }
            }
        }

        return true;
    }

    private AssetFile? ReadAssetFile(AnalysisContext context, FilePath path)
    {
        path = path.GetDirectory().CombineWithFilePath("obj/project.assets.json");

        var assets = _assetFileReader.ReadAssetFile(path);
        if (assets == null)
        {
            context.AddError($"Could not find [yellow]project.assets.json[/] at [yellow]{path}[/]");
            return null;
        }

        return assets;
    }

    private FilePath? GetNuGetPackagePath(AnalysisContext context, AssetFile assets, Library library)
    {
        if (library.Path == null)
        {
            context.AddWarning("Encountered library without a path");
            return null;
        }

        var packagesFolders = new List<DirectoryPath>();
        packagesFolders.AddIfNotNull(assets.Project.Restore.PackagesPath);
        packagesFolders.AddRange(assets.Project.Restore.FallbackFolders);

        foreach (var packagesFolder in packagesFolders)
        {
            if (!_fileSystem.Exist(packagesFolder))
            {
                context.AddWarning($"The packages folder '{packagesFolder}' did not exist on disk");
                continue;
            }

            var packageFolder = packagesFolder.Combine(library.Path);
            var packagePath = _globber.Match("./*.nupkg", new GlobberSettings { Root = packageFolder }).OfType<FilePath>().SingleOrDefault();
            if (packagePath == null)
            {
                context.AddWarning($"The library folder '{packageFolder}' did not contain a NuSpec");
                continue;
            }

            return packagePath;
        }

        return null;
    }

    private (NuGetVersion? Version, string? Copyright) PerformDesignTimeBuild(AnalysisContext context, AssetFile assets)
    {
        if (!context.Cli.GetOption<bool>(DesignTimeBuildFlag))
        {
            return (null, null);
        }

        var analyzer = _analyzerManager.GetProject(assets.Project.Restore.ProjectPath.FullPath);
        var result = analyzer.Build().Results.FirstOrDefault();
        if (result == null)
        {
            return (Version: null, Copyright: null);
        }

        var nugetVersion = default(NuGetVersion);
        if (result.Properties.TryGetValue("AssemblyVersion", out var version))
        {
            nugetVersion = NuGetVersion.Parse(version);
        }

        result.Properties.TryGetValue("Copyright", out var copyright);

        return (nugetVersion, copyright);
    }
}
