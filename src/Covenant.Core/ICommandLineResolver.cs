namespace Covenant.Core;

public interface ICommandLineResolver
{
    T? GetOption<T>(string alias);
}