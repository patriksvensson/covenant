namespace Covenant.Core;

public interface ICovenantMiddleware
{
    int Order { get; }
    Bom Process(MiddlewareContext context, Bom bom);
}