namespace Covenant.Core;

public abstract class BomSerializer : ICovenantInitializable
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Extension { get; }

    public virtual void Initialize(ICommandLineAugmentor cli)
    {
    }

    public abstract string? Serialize(Bom bom, BomSerializerSettings settings, ICommandLineResolver cli);
}