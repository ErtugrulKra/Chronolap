using Chronolap;
using System.Diagnostics;
using System.Linq;

namespace Choronolap.OpenTelemetry
{
    public static class ActivityExtensions
    {
        public static void Lap(this Activity? activity, string lapName, ChronolapTimer timer, string? tagPrefix = "lap")
        {
            if (activity == null || !activity.IsAllDataRequested)
                return;

            var lap = timer.Laps.LastOrDefault(l => l.Name == lapName);
            if (lap == null)
                return;

            var key = $"{tagPrefix}.{lap.Name.Replace(" ", "_")}.duration_ms";
            var value = lap.Duration.TotalMilliseconds;

            activity.SetTag(key, value);
        }

        public static void ExportAllLaps(this Activity? activity, ChronolapTimer timer, string? tagPrefix = "lap")
        {
            if (activity == null || !activity.IsAllDataRequested)
                return;

            foreach (var lap in timer.Laps)
            {
                var key = $"{tagPrefix}.{lap.Name.Replace(" ", "_")}.duration_ms";
                activity.SetTag(key, lap.Duration.TotalMilliseconds);
            }

            activity.SetTag($"{tagPrefix}.total_ms", timer.TotalLapTime.TotalMilliseconds);
        }
    }
}
