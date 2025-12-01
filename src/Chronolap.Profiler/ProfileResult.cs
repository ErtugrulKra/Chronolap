using System;
using System.Collections.Generic;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Represents the result of a profiled method execution.
    /// </summary>
    public class ProfileResult
    {
        /// <summary>
        /// Gets or sets the name of the profiled method.
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the profile measurement.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the duration of the method execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the method started executing.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the method finished executing.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets whether the method execution was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during execution, if any.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with this profile measurement.
        /// </summary>
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets additional metadata for this profile measurement.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the thread ID where the method was executed.
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Gets the duration in milliseconds.
        /// </summary>
        public double DurationMs => Duration.TotalMilliseconds;

        /// <summary>
        /// Returns a string representation of the profile result.
        /// </summary>
        public override string ToString()
        {
            var status = IsSuccess ? "Success" : "Failed";
            var category = !string.IsNullOrEmpty(Category) ? $"[{Category}] " : "";
            var tags = Tags.Count > 0 ? $" ({string.Join(", ", Tags)})" : "";
            
            return $"{category}{MethodName}: {DurationMs:N2} ms - {status}{tags}";
        }
    }
}
