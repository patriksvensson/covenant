namespace Covenant.Analysis.Npm;

internal sealed class NpmAnalyzer : Analyzer
{
    private const string NoDevDependenciesFlag = "--no-dev-dependencies";

    private readonly NpmAssetReader _assetReader;

    public override string[] Patterns { get; } = new[] { "**/package.json" };

    public NpmAnalyzer(IFileSystem fileSystem, IEnvironment environment)
    {
        _assetReader = new NpmAssetReader(fileSystem, environment);
    }

    public override bool ShouldTraverse(DirectoryPath path)
    {
        return !path.Segments.Any(s => s.Equals("node_modules"));
    }

    public override bool CanHandle(AnalysisContext context, FilePath path)
    {
        return path.GetFilename().FullPath.Equals("package.json", StringComparison.OrdinalIgnoreCase) ||
            path.GetFilename().FullPath.Equals("package-lock.json", StringComparison.OrdinalIgnoreCase);
    }

    public override void Initialize(ICommandLineAugmentor cli)
    {
        cli.AddOption<bool>(NoDevDependenciesFlag, "Excludes dev dependencies for NPM projects", false);
    }

    public override void Analyze(AnalysisContext context, FilePath path)
    {
        path = path.GetDirectory().CombineWithFilePath("package.json");

        // Read the asset file
        var assetFile = _assetReader.ReadAssetFile(path);
        if (assetFile == null)
        {
            context.AddError("Could not read package.json");
            return;
        }

        // Add main component
        var root = context.AddComponent(
            new NpmComponent(assetFile.Name!, assetFile.Version!, BomComponentKind.Root));

        // Read the lock file
        var lockPath = path.GetDirectory().CombineWithFilePath("package-lock.json");
        var lockFile = _assetReader.ReadLockFile(lockPath);
        if (lockFile == null)
        {
            context.AddError("Could not read package-lock.json");
            return;
        }

        var optionalPackages = new HashSet<string>();

        // Add all packages
        if (lockFile.Packages != null)
        {
            foreach (var (packagePath, package) in lockFile.Packages)
            {
                // The app has an empty entry in the "packages" section.
                // Don't ask me why, I have no idea.
                if (string.IsNullOrEmpty(packagePath))
                {
                    continue;
                }

                var packageFilePath = lockPath.GetDirectory().Combine(packagePath).CombineWithFilePath("package.json");
                var packageAssets = _assetReader.ReadAssetFile(packageFilePath);
                if (packageAssets == null)
                {
                    if (package.Optional == null || !package.Optional.Value)
                    {
                        context.AddError($"Could not read package.json for [yellow]{packagePath}[/]");
                    }
                    else
                    {
                        optionalPackages.Add(packagePath.Replace("node_modules/", string.Empty));
                    }

                    continue;
                }

                context
                    .AddComponent(
                        new NpmComponent(packageAssets.Name!, package.Version!, BomComponentKind.Library))
                    .SetHash(NpmHashParser.Parse(package.Integrity))
                    .SetLicense(NpmLicenseParser.Parse(packageAssets.License));
            }
        }

        // If we got errors, then abort
        if (context.HasErrors)
        {
            return;
        }

        // Add dependencies to the main application
        if (assetFile.Dependencies != null)
        {
            AddDependencies(context, root, assetFile.Dependencies, optionalPackages);
        }

        // Add dev dependencies to the main application
        if (assetFile.DevDependencies != null)
        {
            // No opt-out?
            if (!context.Cli.GetOption<bool>(NoDevDependenciesFlag))
            {
                AddDependencies(context, root, assetFile.DevDependencies, optionalPackages);
            }
        }

        // Add dependencies of dependencies
        if (lockFile.Dependencies != null)
        {
            foreach (var (packageName, package) in lockFile.Dependencies)
            {
                // Find the package
                var bomPackage = context.Graph.FindByBomRef(NpmComponent.GetBomRef(packageName, package.Version!, BomComponentKind.Library));
                if (bomPackage == null)
                {
                    if (!optionalPackages.Contains(packageName))
                    {
                        context.AddWarning($"Could not find NPM package [yellow]{packageName}[/]");
                    }

                    continue;
                }

                if (package.Requires != null)
                {
                    AddDependencies(context, bomPackage, package.Requires, optionalPackages);
                }
            }
        }
    }

    private static void AddDependencies(AnalysisContext context, BomComponent root, Dictionary<string, string>? dependencies, IReadOnlySet<string> optionalPackages)
    {
        if (dependencies != null)
        {
            foreach (var (dependencyName, dependencyRange) in dependencies)
            {
                var range = new NpmVersionRange(dependencyRange);

                var bomDependency = context.Graph.FindNpmComponent(dependencyName, range, out var foundMatch);
                if (bomDependency != null)
                {
                    if (!foundMatch)
                    {
                        context.AddWarning($"Could not find exact NPM dependency match [yellow]{dependencyName}[/] ({dependencyRange})");
                    }

                    context.Connect(root, bomDependency);
                }
                else
                {
                    if (optionalPackages?.Contains(dependencyName) == false)
                    {
                        context.AddWarning($"Could not find NPM dependency [yellow]{dependencyName}[/] ({dependencyRange})");
                    }
                }
            }
        }
    }
}
