using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Chronolap
{
    public class ChronolapOptions
    {
        public int MaxLapCount { get; set; } = 1000;
        public int MinimumLapCountForStatistics { get; set; } = 30;
    }

    public static class ChronolapServiceCollectionExtensions
    {
        public static IServiceCollection AddChronolap(this IServiceCollection services)
        {
            return AddChronolap(services, _ => { });
        }

        public static IServiceCollection AddChronolap(this IServiceCollection services, Action<ChronolapOptions> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new ChronolapOptions();
            configure(options);

            services.TryAddTransient<ChronolapTimer>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ChronolapTimer>>();
                
                if (logger != null)
                {
                    return new ChronolapTimer(logger, options.MaxLapCount, options.MinimumLapCountForStatistics);
                }
                
                return new ChronolapTimer(options.MaxLapCount, options.MinimumLapCountForStatistics);
            });

            return services;
        }

        public static IServiceCollection AddChronolap(this IServiceCollection services, ChronolapOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.TryAddTransient<ChronolapTimer>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ChronolapTimer>>();
                
                if (logger != null)
                {
                    return new ChronolapTimer(logger, options.MaxLapCount, options.MinimumLapCountForStatistics);
                }
                
                return new ChronolapTimer(options.MaxLapCount, options.MinimumLapCountForStatistics);
            });

            return services;
        }
    }
}

