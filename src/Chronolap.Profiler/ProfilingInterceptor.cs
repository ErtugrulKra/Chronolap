using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Interceptor for automatic method profiling using Castle.DynamicProxy.
    /// </summary>
    public class ProfilingInterceptor : IInterceptor
    {
        private readonly ChronolapProfiler _profiler;
        private readonly ILogger? _logger;

        public ProfilingInterceptor(ChronolapProfiler profiler, ILogger? logger = null)
        {
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var attribute = method.GetCustomAttribute<ChronolapProfileAttribute>();

            // If no attribute, just proceed without profiling
            if (attribute == null)
            {
                invocation.Proceed();
                return;
            }

            // If profiler is disabled, just proceed
            if (!_profiler.IsEnabled)
            {
                invocation.Proceed();
                return;
            }

            // Create profile result
            var result = new ProfileResult
            {
                MethodName = attribute.Name ?? method.Name,
                Category = attribute.Category,
                StartTime = DateTime.UtcNow,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            // Add tags
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
                invocation.Proceed();
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

                // Re-throw if we shouldn't record on exception
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

                // Only record if meets minimum duration
                if (result.DurationMs >= attribute.MinimumDurationMs)
                {
                    RecordResult(result);
                }
            }
        }

        private void RecordResult(ProfileResult result)
        {
            _profiler.AddResult(result);

            _logger?.LogDebug(
                "Profile: {MethodName} - {Duration} ms - {Status}",
                result.MethodName,
                result.DurationMs,
                result.IsSuccess ? "Success" : "Failed"
            );
        }
    }
}
