namespace Covenant;

internal sealed class CovenantConfigurationReader
{
    private readonly IFileSystem _fileSystem;

    public CovenantConfigurationReader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public CovenantConfiguration? Read(FilePath path)
    {
        if (!_fileSystem.Exist(path))
        {
            return null;
        }

        var json = _fileSystem.ReadAllText(path);
        return JsonConvert.DeserializeObject<CovenantConfiguration>(json);
    }
}
