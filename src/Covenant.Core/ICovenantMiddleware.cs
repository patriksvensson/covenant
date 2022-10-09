namespace Covenant.Core;

public interface ICovenantMiddleware
{
    Bom Process(Bom bom, ICommandLineResolver cli);
}