var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

////////////////////////////////////////////////////////////////
// Tasks

Task("Build")
    .Does(context => 
{
    DotNetBuild("./src/Covenant.sln", new DotNetBuildSettings {
        Configuration = configuration,
        NoIncremental = context.HasArgument("rebuild"),
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(context => 
{
    DotNetTest("./src/Covenant.sln", new DotNetTestSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(context => 
{
    CleanDirectory("./.artifacts");

    DotNetPack("./src/Covenant.sln", new DotNetPackSettings {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
            .WithProperty("SymbolPackageFormat", "snupkg")
    });
});

Task("Publish")
    .WithCriteria(ctx => BuildSystem.IsRunningOnGitHubActions, "Not running on GitHub Actions")
    .IsDependentOn("Pack")
    .Does(context => 
{
    var apiKey = Argument<string>("nuget-key", null);
    if(string.IsNullOrWhiteSpace(apiKey)) {
        throw new CakeException("No NuGet API key was provided.");
    }

    // Publish to GitHub Packages
    foreach(var file in context.GetFiles("./.artifacts/*.nupkg")) 
    {
        Information("Publishing {0}...", file.GetFilename().FullPath);
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = apiKey,
        });
    }
});

////////////////////////////////////////////////////////////////
// Targets

Task("Default")
    .IsDependentOn("Pack");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target)