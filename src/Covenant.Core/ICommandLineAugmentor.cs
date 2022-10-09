namespace Covenant.Core;

public interface ICommandLineAugmentor
{
    void AddOption<T>(string alias, string description, object? defaultValue);
}