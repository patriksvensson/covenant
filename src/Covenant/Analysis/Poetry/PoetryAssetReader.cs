namespace Covenant.Analysis.Poetry;

internal sealed class PoetryAssetReader
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;

    public PoetryAssetReader(IFileSystem fileSystem, IEnvironment environment)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public PyProjectToml? ReadAssetFile(FilePath path)
    {
        var file = _fileSystem.GetFile(path.MakeAbsolute(_environment));
        if (!file.Exists)
        {
            return null;
        }

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            var toml = reader.ReadToEnd();
            var pyProject = Tomlyn.Toml.ToModel<PyProjectToml>(toml);

            return pyProject;
        }
    }

    public PoetryLock? ReadLockFile(FilePath path)
    {
        var file = _fileSystem.GetFile(path.MakeAbsolute(_environment));
        if (!file.Exists)
        {
            return null;
        }

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            var toml = reader.ReadToEnd();
            var poetryLockfile = Tomlyn.Toml.ToModel<PoetryLock>(toml);

            return poetryLockfile;
        }
    }
}