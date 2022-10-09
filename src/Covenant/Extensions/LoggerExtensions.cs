namespace Covenant;

public static class LoggerExtensions
{
    public static void Info(this ILogger logger, string message)
    {
        logger.Write(LogLevel.Information, message);
    }

    public static void Warning(this ILogger logger, string message)
    {
        logger.Write(LogLevel.Warning, message);
    }

    public static void Error(this ILogger logger, string message)
    {
        logger.Write(LogLevel.Error, message);
    }
}