namespace Covenant.Infrastructure;

public interface ILogger
{
    void Write(LogLevel level, string text);
}