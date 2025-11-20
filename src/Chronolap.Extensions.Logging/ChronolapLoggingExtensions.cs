namespace Chronolap.Extensions.Logging
{
    using Microsoft.Extensions.Logging;
    using System.Text;

    public static class ChronolapLoggingExtensions
    {
        public static void LogLaps(this ILogger logger, ChronolapTimer chrono,
                                   LogLevel level = LogLevel.Debug,
                                   string? formatter = null)
        {
            if (chrono == null || logger == null) return;

            var laps = chrono.Laps;
            if (laps.Count == 0)
            {
                logger.Log(level, "No laps recorded.");
                return;
            }

            var logMessage = new StringBuilder();
            logMessage.AppendLine("Chronolap Results:");

            for (int i = 0; i < laps.Count; i++)
            {
                var elapsed = laps[i];
                var formatted = formatter == null
                    ? $"Lap {i + 1}: {elapsed:c}"
                    : string.Format(formatter, i + 1, elapsed);

                logMessage.AppendLine(formatted);
            }

            logger.Log(level, logMessage.ToString());
        }
    }
}
