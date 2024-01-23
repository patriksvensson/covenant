namespace Covenant.Analysis.Poetry;

internal class PoetryAnalyzer : Analyzer
{
    private const string NoDevDependenciesFlag = "--no-poetry-dev-dependencies";
    private const string NoTestDependenciesFlag = "--no-poetry-test-dependencies";
    private const string DisablePoetry = "--disable-poetry";

    private readonly PoetryAssetReader _assetReader;
    private bool _enabled = true;

    public override bool Enabled => _enabled;
    public override string[] Patterns { get; } = new[] { "**/pyproject.toml" };

    public PoetryAnalyzer(IFileSystem fileSystem, IEnvironment environment)
    {
        _assetReader = new PoetryAssetReader(fileSystem, environment);
    }

    public override void AfterAnalysis(AnalysisSettings settings)
    {
        base.AfterAnalysis(settings);
    }

    public override void Analyze(AnalysisContext context, FilePath path)
    {
        path = path.GetDirectory().CombineWithFilePath("pyproject.toml");

        // Read the asset file
        var assetFile = _assetReader.ReadAssetFile(path);
        if (assetFile == null)
        {
            context.AddError("Could not read pyproject.toml");
            return;
        }

        // Add main component
        var root = context.AddComponent(
            new PoetryComponent(assetFile.Tool.Poetry.Name!, assetFile.Tool.Poetry.Version!, BomComponentKind.Root));

        // Read the lock file
        var lockPath = path.GetDirectory().CombineWithFilePath("poetry.lock");
        var lockFile = _assetReader.ReadLockFile(lockPath);
        if (lockFile == null)
        {
            context.AddError("Could not read poetry.lock");
            return;
        }

        var optionalPackages = new HashSet<string>();

        // Add all packages
        if (lockFile.Packages != null)
        {
            foreach (var package in lockFile.Packages)
            {
                // var packageFilePath = lockPath.GetDirectory().Combine(packagePath).CombineWithFilePath("package.json");
                // var packageAssets = _assetReader.ReadAssetFile(packageFilePath);
                // if (packageAssets == null)
                // {
                //     if (package.Optional == null || !package.Optional.Value)
                //     {
                //         context.AddError($"Could not read package.json for [yellow]{packagePath}[/]");
                //     }
                //     else
                //     {
                //         optionalPackages.Add(packagePath.Replace("node_modules/", string.Empty));
                //     }

                //     continue;
                // }

                // TODO:
                //  - The lock file doesn't include license info, or a link to the source of the package
                //  - We have hash info for multiple files (e.g. wheel, source, architecture-specific versions etc.)
                context
                    .AddComponent(
                        new PoetryComponent(package.Name!, package.Version!, BomComponentKind.Library))
                    .SetHash(PoetryHashParser.Parse(package.Files.First(f => f.File.EndsWith(".whl")).Hash))
                    .SetLicense(new BomLicense
                    {
                        Id = "Unknown",
                        Name = "Unknown",
                    });
            }
        }

        // If we got errors, then abort
        if (context.HasErrors)
        {
            return;
        }

        // Add dependencies to the main application
        if (assetFile.Tool.Poetry.Dependencies != null)
        {
            AddDependencies(context, root, assetFile.Tool.Poetry.Dependencies, optionalPackages);
        }

        // Add dev dependencies to the main application
        if (assetFile.Tool.Poetry.Groups.Dev != null)
        {
            // No opt-out?
            if (!context.Cli.GetOption<bool>(NoDevDependenciesFlag))
            {
                AddDependencies(context, root, assetFile.Tool.Poetry.Groups.Dev, optionalPackages);
            }
        }

        // Add test dependencies to the main application
        if (assetFile.Tool.Poetry.Groups.Test != null)
        {
            // No opt-out?
            if (!context.Cli.GetOption<bool>(NoTestDependenciesFlag))
            {
                AddDependencies(context, root, assetFile.Tool.Poetry.Groups.Test, optionalPackages);
            }
        }

        // Add dependencies of dependencies
        // if (lockFile.Dependencies != null)
        // {
        //     foreach (var (packageName, package) in lockFile.Dependencies)
        //     {
        //         // Find the package
        //         var bomPackage = context.Graph.FindByBomRef(NpmComponent.GetBomRef(packageName, package.Version!, BomComponentKind.Library));
        //         if (bomPackage == null)
        //         {
        //             if (!optionalPackages.Contains(packageName))
        //             {
        //                 context.AddWarning($"Could not find NPM package [yellow]{packageName}[/]");
        //             }

        //             continue;
        //         }

        //         if (package.Requires != null)
        //         {
        //             AddDependencies(context, bomPackage, package.Requires, optionalPackages);
        //         }
        //     }
        // }
    }

    public override void BeforeAnalysis(AnalysisSettings settings)
    {
        if (settings.Cli.GetOption<bool>(DisablePoetry))
        {
            _enabled = false;
        }
    }

    public override bool CanHandle(AnalysisContext context, FilePath path)
    {
        return path.GetFilename().FullPath.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase) ||
            path.GetFilename().FullPath.Equals("poetry.lock", StringComparison.OrdinalIgnoreCase);
    }

    public override void Initialize(ICommandLineAugmentor cli)
    {
        cli.AddOption<bool>(NoDevDependenciesFlag, "Excludes dev dependencies for Python Poetry projects", false);
        cli.AddOption<bool>(NoTestDependenciesFlag, "Excludes test dependencies for Python Poetry projects", false);
        cli.AddOption<bool>(DisablePoetry, "Disables the Python Poetry analyzer", false);
    }

    public override bool ShouldTraverse(DirectoryPath path)
    {
        return base.ShouldTraverse(path);
    }


    private static void AddDependencies(AnalysisContext context, BomComponent root, PyProjectToolPoetryGroupDependencies? dependencies, IReadOnlySet<string> optionalPackages)
    {
        // TODO
    }
    
    private static void AddDependencies(AnalysisContext context, BomComponent root, Dictionary<string, string>? dependencies, IReadOnlySet<string> optionalPackages)
    {
        // if (dependencies != null)
        // {
        //     foreach (var (dependencyName, dependencyRange) in dependencies)
        //     {
        //         var range = new NpmVersionRange(dependencyRange);

        //         var bomDependency = context.Graph.FindNpmComponent(dependencyName, range, out var foundMatch);
        //         if (bomDependency != null)
        //         {
        //             if (!foundMatch)
        //             {
        //                 context.AddWarning($"Could not find exact NPM dependency match [yellow]{dependencyName}[/] ({dependencyRange})");
        //             }

        //             context.Connect(root, bomDependency);
        //         }
        //         else
        //         {
        //             if (optionalPackages?.Contains(dependencyName) == false)
        //             {
        //                 context.AddWarning($"Could not find NPM dependency [yellow]{dependencyName}[/] ({dependencyRange})");
        //             }
        //         }
        //     }
        // }
    }
}
