using Covenant.Analysis.Poetry;
using Shouldly;
using Spectre.IO;

namespace Covenant.Tests;

public class PythonPoetryTests
{
    [Fact]
    public void Should_Parse_Poetry_Project_Correctly()
    {
        var project = new PoetryAssetReader(new FileSystem(), new Spectre.IO.Environment());
        project.ReadAssetFile(@"./test-data/PythonPoetry/pyproject.toml");
    }

    [Fact]
    public void Should_Parse_Poetry_Lockfile_Correctly()
    {
        var project = new PoetryAssetReader(new FileSystem(), new Spectre.IO.Environment());
        project.ReadLockFile(@"./test-data/PythonPoetry/poetry.lock");
    }
}