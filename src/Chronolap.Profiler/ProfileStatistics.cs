using System;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Represents statistical information about profiled method executions.
    /// </summary>
    public class ProfileStatistics
    {
        /// <summary>
        /// Gets or sets the name of the profiled method.
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of calls to this method.
        /// </summary>
        public int TotalCalls { get; set; }

        /// <summary>
        /// Gets or sets the number of successful executions.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the number of failed executions.
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Gets the success rate as a percentage.
        /// </summary>
        public double SuccessRate => TotalCalls > 0 ? (SuccessCount / (double)TotalCalls) * 100 : 0;

        /// <summary>
        /// Gets or sets the minimum execution duration.
        /// </summary>
        public TimeSpan MinDuration { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution duration.
        /// </summary>
        public TimeSpan MaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the average execution duration.
        /// </summary>
        public TimeSpan AverageDuration { get; set; }

        /// <summary>
        /// Gets or sets the median execution duration.
        /// </summary>
        public TimeSpan MedianDuration { get; set; }

        /// <summary>
        /// Gets or sets the total execution duration across all calls.
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Returns a string representation of the profile statistics.
        /// </summary>
        public override string ToString()
        {
            return $"{MethodName}: {TotalCalls} calls, Avg: {AverageDuration.TotalMilliseconds:N2} ms, " +
                   $"Success Rate: {SuccessRate:N1}%";
        }
    }
}
