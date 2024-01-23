using System.Runtime.Serialization;

namespace Covenant.Analysis.Poetry;
internal sealed class PoetryLock
{
    [DataMember(Name = "package")]
    public List<PoetryLockPackage>? Packages { get; set; }

    public PoetryLockMetadata? Metadata { get; set; }
}

internal sealed class PoetryLockPackage
{
    public string? Name { get; set; }

    public string? Version { get; set; }

    public string? Description { get; set; }

    public bool? Optional { get; set; }

    [DataMember(Name = "python-versions")]
    public string? PythonVersions { get; set; }

    public List<PoetryLockPackageFile>? Files { get; set; }

    public Dictionary<string, object>? Dependencies { get; set; }

    public Dictionary<string, List<string>>? Extras { get; set; }
}

internal sealed class PoetryLockPackageDependencyComplex
{
    public string? Version { get; set; }

    public string? Markers { get; set; }
}

internal sealed class PoetryLockPackageFile
{
    public string? File { get; set; }

    public string? Hash { get; set; }
}

internal sealed class PoetryLockMetadata
{
    [DataMember(Name = "lock-version")]
    public string? LockVersion { get; set; }

    [DataMember(Name = "python-versions")]
    public string? PythonVersions { get; set; }

    [DataMember(Name = "content-hash")]
    public string? ContentHash { get; set; }
}
