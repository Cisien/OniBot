using System;
using Microsoft.Extensions.Logging;

namespace OniBot
{
    public static class LoggerExtensions
    {
        public static void LogError(this ILogger logger, Exception ex)
        {
            logger.LogError(new EventId(), ex, ex.Message);
        }

        public static void LogDebug(this ILogger logger, Exception ex)
        {
            logger.LogDebug(new EventId(), ex, ex.Message);
        }

        public static void LogWarning(this ILogger logger, Exception ex)
        {
            logger.LogWarning(new EventId(), ex, ex.Message);
        }

        public static void LogInformation(this ILogger logger, Exception ex)
        {
            logger.LogInformation(new EventId(), ex, ex.Message);
        }
    }
}
