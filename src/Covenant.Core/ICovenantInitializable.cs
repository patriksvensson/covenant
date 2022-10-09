namespace Covenant.Core;

public interface ICovenantInitializable
{
    void Initialize(ICommandLineAugmentor cli);
}