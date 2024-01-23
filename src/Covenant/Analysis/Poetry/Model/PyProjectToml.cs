using System.Runtime.Serialization;

namespace Covenant.Analysis.Poetry;
internal sealed class PyProjectToml
{
    public PyProjectTool? Tool { get; set; }

    [DataMember(Name = "build-system")]
    public PyProjectBuildSystem? BuildSystem { get; set; }
}

internal sealed class PyProjectBuildSystem
{
    public List<string>? Requires { get; set; }

    [DataMember(Name = "build-backend")]
    public string? BuildBackend { get; set; }
}

internal sealed class PyProjectTool
{
    public PyProjectToolPoetry? Poetry { get; set; }
}

internal sealed class PyProjectToolPoetry
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Version { get; set; }

    public string? License { get; set; }

    public List<string>? Authors { get; set; }

    public Dictionary<string, string>? Dependencies { get; set; }

    [DataMember(Name = "group")]
    public PyProjectToolPoetryGroups? Groups { get; set; }
}

internal sealed class PyProjectToolPoetryGroups
{
    public PyProjectToolPoetryGroupDependencies? Dev { get; set; }
    public PyProjectToolPoetryGroupDependencies? Test { get; set; }
}

internal sealed class PyProjectToolPoetryGroupDependencies
{
    [DataMember(Name = "dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; }
}
