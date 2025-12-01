using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace Chronolap.Profiler
{
    /// <summary>
    /// Extension methods for registering ChronolapProfiler with dependency injection.
    /// </summary>
    public static class ChronolapProfilerExtensions
    {
        /// <summary>
        /// Adds ChronolapProfiler as a singleton service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddChronolapProfiler(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ChronolapProfiler>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ChronolapProfiler>>();
                return logger != null ? new ChronolapProfiler(logger) : new ChronolapProfiler();
            });

            services.TryAddSingleton<ProfilingInterceptor>(serviceProvider =>
            {
                var profiler = serviceProvider.GetRequiredService<ChronolapProfiler>();
                var logger = serviceProvider.GetService<ILogger<ProfilingInterceptor>>();
                return new ProfilingInterceptor(profiler, logger);
            });

            services.TryAddSingleton<IProxyGenerator>(new ProxyGenerator());

            return services;
        }

        /// <summary>
        /// Adds ChronolapProfiler as a singleton service with configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Configuration action for the profiler.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddChronolapProfiler(
            this IServiceCollection services, 
            Action<ChronolapProfiler> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services.TryAddSingleton<ChronolapProfiler>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<ChronolapProfiler>>();
                var profiler = logger != null ? new ChronolapProfiler(logger) : new ChronolapProfiler();
                configure(profiler);
                return profiler;
            });

            services.TryAddSingleton<ProfilingInterceptor>(serviceProvider =>
            {
                var profiler = serviceProvider.GetRequiredService<ChronolapProfiler>();
                var logger = serviceProvider.GetService<ILogger<ProfilingInterceptor>>();
                return new ProfilingInterceptor(profiler, logger);
            });

            services.TryAddSingleton<IProxyGenerator>(new ProxyGenerator());

            return services;
        }

        /// <summary>
        /// Registers a service with profiling support using AOP.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddProfiledTransient<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddTransient<TImplementation>();
            services.AddTransient<TInterface>(serviceProvider =>
            {
                var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
                var interceptor = serviceProvider.GetRequiredService<ProfilingInterceptor>();
                var target = serviceProvider.GetRequiredService<TImplementation>();

                return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
            });

            return services;
        }

        /// <summary>
        /// Registers a service with profiling support using AOP as singleton.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddProfiledSingleton<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton<TInterface>(serviceProvider =>
            {
                var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
                var interceptor = serviceProvider.GetRequiredService<ProfilingInterceptor>();
                var target = serviceProvider.GetRequiredService<TImplementation>();

                return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
            });

            return services;
        }

        /// <summary>
        /// Registers a service with profiling support using AOP as scoped.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddProfiledScoped<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TImplementation>();
            services.AddScoped<TInterface>(serviceProvider =>
            {
                var proxyGenerator = serviceProvider.GetRequiredService<IProxyGenerator>();
                var interceptor = serviceProvider.GetRequiredService<ProfilingInterceptor>();
                var target = serviceProvider.GetRequiredService<TImplementation>();

                return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
            });

            return services;
        }
    }
}
