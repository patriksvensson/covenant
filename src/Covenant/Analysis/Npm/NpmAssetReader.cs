namespace Covenant.Analysis.Npm;

internal sealed class NpmAssetReader
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;

    public NpmAssetReader(IFileSystem fileSystem, IEnvironment environment)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public PackageJson? ReadAssetFile(FilePath path)
    {
        var file = _fileSystem.GetFile(path.MakeAbsolute(_environment));
        if (!file.Exists)
        {
            return null;
        }

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<PackageJson>(json);
        }
    }

    public PackageLockJson? ReadLockFile(FilePath path)
    {
        var file = _fileSystem.GetFile(path.MakeAbsolute(_environment));
        if (!file.Exists)
        {
            return null;
        }

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<PackageLockJson>(json);
        }
    }
}
