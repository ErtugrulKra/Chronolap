using System;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Marks a method for automatic profiling with ChronolapProfiler.
    /// When applied to a method, execution time will be automatically tracked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ChronolapProfileAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the custom name for this profile measurement.
        /// If not specified, the method name will be used.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the category for grouping related measurements.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets whether to log exceptions that occur during method execution.
        /// Default is true.
        /// </summary>
        public bool LogExceptions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include method parameters in the profile data.
        /// Default is false for performance and privacy reasons.
        /// </summary>
        public bool IncludeParameters { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum execution time (in milliseconds) to record.
        /// Measurements below this threshold will be ignored.
        /// Default is 0 (record all measurements).
        /// </summary>
        public double MinimumDurationMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether to record this measurement even when an exception occurs.
        /// Default is true.
        /// </summary>
        public bool RecordOnException { get; set; } = true;

        /// <summary>
        /// Gets or sets custom tags to associate with this profile measurement.
        /// Multiple tags should be separated by comma.
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChronolapProfileAttribute class.
        /// </summary>
        public ChronolapProfileAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChronolapProfileAttribute class with a custom name.
        /// </summary>
        /// <param name="name">The custom name for this profile measurement.</param>
        public ChronolapProfileAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the ChronolapProfileAttribute class with a custom name and category.
        /// </summary>
        /// <param name="name">The custom name for this profile measurement.</param>
        /// <param name="category">The category for grouping related measurements.</param>
        public ChronolapProfileAttribute(string name, string category)
        {
            Name = name;
            Category = category;
        }
    }
}
