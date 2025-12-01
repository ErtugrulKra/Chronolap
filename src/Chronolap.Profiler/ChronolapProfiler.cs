using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Provides automatic method profiling capabilities using ChronolapProfileAttribute.
    /// </summary>
    public class ChronolapProfiler
    {
        private readonly ConcurrentBag<ProfileResult> _results;
        private readonly ILogger? _logger;
        private readonly object _lockObject = new object();
        private bool _isEnabled = true;

        /// <summary>
        /// Gets whether the profiler is currently enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Gets all profile results.
        /// </summary>
        public IReadOnlyList<ProfileResult> Results
        {
            get
            {
                lock (_lockObject)
                {
                    return _results.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Gets the total number of recorded profile results.
        /// </summary>
        public int Count => _results.Count;

        /// <summary>
        /// Initializes a new instance of the ChronolapProfiler class.
        /// </summary>
        public ChronolapProfiler()
        {
            _results = new ConcurrentBag<ProfileResult>();
        }

        /// <summary>
        /// Initializes a new instance of the ChronolapProfiler class with logging support.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ChronolapProfiler(ILogger logger) : this()
        {
            _logger = logger;
        }

        /// <summary>
        /// Profiles a synchronous method execution.
        /// </summary>
        /// <param name="action">The action to profile.</param>
        /// <param name="attribute">The profiling attribute configuration.</param>
        /// <param name="methodName">The name of the method being profiled.</param>
        public void Profile(Action action, ChronolapProfileAttribute attribute, string methodName)
        {
            if (!_isEnabled)
            {
                action();
                return;
            }

            var result = new ProfileResult
            {
                MethodName = attribute.Name ?? methodName,
                Category = attribute.Category,
                StartTime = DateTime.UtcNow,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            if (!string.IsNullOrEmpty(attribute.Tags))
            {
                foreach (var tag in attribute.Tags.Split(','))
                {
                    result.Tags.Add(tag.Trim());
                }
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                action();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Exception = ex;

                if (attribute.LogExceptions && _logger != null)
                {
                    _logger.LogError(ex, "Exception occurred while profiling method: {MethodName}", result.MethodName);
                }

                if (!attribute.RecordOnException)
                {
                    throw;
                }

                throw;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.EndTime = DateTime.UtcNow;

                if (result.DurationMs >= attribute.MinimumDurationMs)
                {
                    RecordResult(result);
                }
            }
        }

        /// <summary>
        /// Profiles a synchronous method execution with a return value.
        /// </summary>
        /// <typeparam name="T">The return type of the method.</typeparam>
        /// <param name="func">The function to profile.</param>
        /// <param name="attribute">The profiling attribute configuration.</param>
        /// <param name="methodName">The name of the method being profiled.</param>
        /// <returns>The result of the function execution.</returns>
        public T Profile<T>(Func<T> func, ChronolapProfileAttribute attribute, string methodName)
        {
            if (!_isEnabled)
            {
                return func();
            }

            var result = new ProfileResult
            {
                MethodName = attribute.Name ?? methodName,
                Category = attribute.Category,
                StartTime = DateTime.UtcNow,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            if (!string.IsNullOrEmpty(attribute.Tags))
            {
                foreach (var tag in attribute.Tags.Split(','))
                {
                    result.Tags.Add(tag.Trim());
                }
            }

            var stopwatch = Stopwatch.StartNew();
            T returnValue;

            try
            {
                returnValue = func();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Exception = ex;

                if (attribute.LogExceptions && _logger != null)
                {
                    _logger.LogError(ex, "Exception occurred while profiling method: {MethodName}", result.MethodName);
                }

                if (!attribute.RecordOnException)
                {
                    throw;
                }

                throw;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.EndTime = DateTime.UtcNow;

                if (result.DurationMs >= attribute.MinimumDurationMs)
                {
                    RecordResult(result);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Records a profile result.
        /// </summary>
        /// <param name="result">The profile result to record.</param>
        private void RecordResult(ProfileResult result)
        {
            _results.Add(result);

            _logger?.LogDebug(
                "Profile: {MethodName} - {Duration} ms - {Status}",
                result.MethodName,
                result.DurationMs,
                result.IsSuccess ? "Success" : "Failed"
            );
        }

        /// <summary>
        /// Adds a profile result (used by interceptors).
        /// </summary>
        /// <param name="result">The profile result to add.</param>
        internal void AddResult(ProfileResult result)
        {
            _results.Add(result);
        }

        /// <summary>
        /// Clears all recorded profile results.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                while (_results.TryTake(out _)) { }
            }
        }

        /// <summary>
        /// Gets profile results filtered by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>Filtered profile results.</returns>
        public IReadOnlyList<ProfileResult> GetResultsByCategory(string category)
        {
            return _results
                .Where(r => string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets profile results filtered by tag.
        /// </summary>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>Filtered profile results.</returns>
        public IReadOnlyList<ProfileResult> GetResultsByTag(string tag)
        {
            return _results
                .Where(r => r.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets profile results filtered by method name.
        /// </summary>
        /// <param name="methodName">The method name to filter by.</param>
        /// <returns>Filtered profile results.</returns>
        public IReadOnlyList<ProfileResult> GetResultsByMethodName(string methodName)
        {
            return _results
                .Where(r => string.Equals(r.MethodName, methodName, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets statistics for a specific method.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <returns>Profile statistics.</returns>
        public ProfileStatistics? GetStatistics(string methodName)
        {
            var methodResults = GetResultsByMethodName(methodName);
            
            if (methodResults.Count == 0)
                return null;

            var durations = methodResults.Select(r => r.DurationMs).OrderBy(d => d).ToList();
            var successCount = methodResults.Count(r => r.IsSuccess);
            var failureCount = methodResults.Count - successCount;

            return new ProfileStatistics
            {
                MethodName = methodName,
                TotalCalls = methodResults.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                MinDuration = TimeSpan.FromMilliseconds(durations.Min()),
                MaxDuration = TimeSpan.FromMilliseconds(durations.Max()),
                AverageDuration = TimeSpan.FromMilliseconds(durations.Average()),
                MedianDuration = TimeSpan.FromMilliseconds(CalculateMedian(durations)),
                TotalDuration = TimeSpan.FromMilliseconds(durations.Sum())
            };
        }

        /// <summary>
        /// Gets statistics for all profiled methods.
        /// </summary>
        /// <returns>List of profile statistics grouped by method name.</returns>
        public IReadOnlyList<ProfileStatistics> GetAllStatistics()
        {
            return _results
                .GroupBy(r => r.MethodName)
                .Select(g => GetStatistics(g.Key))
                .Where(s => s != null)
                .Cast<ProfileStatistics>()
                .ToList()
                .AsReadOnly();
        }

        private static double CalculateMedian(List<double> sortedValues)
        {
            if (sortedValues.Count == 0)
                return 0;

            int count = sortedValues.Count;
            if (count % 2 == 0)
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            else
                return sortedValues[count / 2];
        }

        /// <summary>
        /// Exports all profile results as a summary string.
        /// </summary>
        /// <returns>A formatted summary of all profile results.</returns>
        public string ExportSummary()
        {
            var stats = GetAllStatistics();
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine("=== ChronolapProfiler Summary ===");
            summary.AppendLine($"Total Methods Profiled: {stats.Count}");
            summary.AppendLine($"Total Calls: {_results.Count}");
            summary.AppendLine();

            foreach (var stat in stats.OrderByDescending(s => s.TotalDuration))
            {
                summary.AppendLine($"Method: {stat.MethodName}");
                summary.AppendLine($"  Calls: {stat.TotalCalls} (Success: {stat.SuccessCount}, Failed: {stat.FailureCount})");
                summary.AppendLine($"  Min: {stat.MinDuration.TotalMilliseconds:N2} ms");
                summary.AppendLine($"  Max: {stat.MaxDuration.TotalMilliseconds:N2} ms");
                summary.AppendLine($"  Avg: {stat.AverageDuration.TotalMilliseconds:N2} ms");
                summary.AppendLine($"  Median: {stat.MedianDuration.TotalMilliseconds:N2} ms");
                summary.AppendLine($"  Total: {stat.TotalDuration.TotalMilliseconds:N2} ms");
                summary.AppendLine();
            }

            return summary.ToString();
        }
    }
}
