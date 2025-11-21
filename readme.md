# Chronolap

Advanced stopwatch library with lap tracking support for .NET developers.

## Supported Frameworks

[![.NET Core 3.0+](https://img.shields.io/badge/.NET_Core-3.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/3.0)  
[![.NET 5+](https://img.shields.io/badge/.NET-5.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)  
[![.NET 6+](https://img.shields.io/badge/.NET-6.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)  
[![.NET 7+](https://img.shields.io/badge/.NET-7.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)  
[![.NET Standard 2.1](https://img.shields.io/badge/.NET_Standard-2.1-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)

## Features

- Lap tracking with configurable maximum lap count
- Measurement time recording (synchronous and asynchronous)
- Return value support in measurement methods
- Exception handling support (measurements are recorded even when exceptions occur)
- Pause / Resume functionality
- Advanced statistics (Min, Max, Mean, Median, Standard Deviation, Variance, Percentiles)
- Fastest/Slowest lap detection
- ILogger logging support
- OpenTelemetry Activity integration
- Configurable minimum lap count for statistics
- Cached total lap time calculation for optimal performance
- **Thread-safe** - Safe to use in multi-threaded environments  


## Installation

Install via NuGet:

```bash
dotnet add package Chronolap
```


## Usage

### Basic Usage

```csharp
using Chronolap;
using System;
using System.Threading;

class Program
{
    static void Main()
    {
        var timer = new ChronolapTimer();

        timer.Start();
        Thread.Sleep(100);
        timer.Lap("First lap");
        Thread.Sleep(200);
        timer.Lap("Second lap");
        timer.Stop();

        foreach (var lap in timer.Laps)
        {
            Console.WriteLine(lap);
        }
    }
}
```

### Measurement with Return Values

```csharp
var timer = new ChronolapTimer();
timer.Start();

// Measure and get return value
int result = timer.MeasureExecutionTime(() => CalculateValue(), "Calculation");
Console.WriteLine($"Result: {result}");

// Async measurement with return value
var data = await timer.MeasureExecutionTimeAsync(async () => 
{
    return await FetchDataAsync();
}, "DataFetch");
```

### Exception Handling

```csharp
var timer = new ChronolapTimer();
timer.Start();

// Lap is recorded even if exception occurs
try
{
    timer.MeasureExecutionTimeWithExceptionHandling(() => 
    {
        RiskyOperation();
    }, "RiskyOperation");
}
catch (Exception ex)
{
    // Lap was still recorded
    Console.WriteLine($"Operation failed but timing was recorded");
}

// With return value
int? result = null;
try
{
    result = timer.MeasureExecutionTimeWithExceptionHandling(() => 
    {
        return RiskyOperationWithReturn();
    }, "RiskyOperation");
}
finally
{
    // Timing is always recorded
}
```

### Advanced Statistics

```csharp
var timer = new ChronolapTimer();
timer.Start();

// Record multiple laps
for (int i = 0; i < 50; i++)
{
    Thread.Sleep(10 + i);
    timer.Lap($"Lap{i}");
}

// Calculate statistics
var min = timer.CalculateLapStatistic(LapStatisticsType.Min);
var max = timer.CalculateLapStatistic(LapStatisticsType.Max);
var mean = timer.CalculateLapStatistic(LapStatisticsType.ArithmeticMean);
var median = timer.CalculateLapStatistic(LapStatisticsType.Median);
var stdDev = timer.CalculateLapStatistic(LapStatisticsType.StandardDeviation);
var variance = timer.CalculateLapStatistic(LapStatisticsType.Variance);

// Calculate percentiles
var p50 = timer.CalculatePercentile(50);  // Median
var p95 = timer.CalculatePercentile(95);  // 95th percentile
var p99 = timer.CalculatePercentile(99);  // 99th percentile

// Find fastest and slowest laps
var fastest = timer.GetFastestLap();
var slowest = timer.GetSlowestLap();

Console.WriteLine($"Fastest: {fastest?.Name} ({fastest?.Duration.TotalMilliseconds} ms)");
Console.WriteLine($"Slowest: {slowest?.Name} ({slowest?.Duration.TotalMilliseconds} ms)");
```

### Configuration

```csharp
// Configure maximum lap count and minimum lap count for statistics
var timer = new ChronolapTimer(
    maxLapCount: 5000, 
    minimumLapCountForStatistics: 50
);

// Or change minimum lap count at runtime
timer.MinimumLapCountForStatistics = 100;

// Access configuration
Console.WriteLine($"Max Lap Count: {timer.MaxLapCount}");
Console.WriteLine($"Min Lap Count for Stats: {timer.MinimumLapCountForStatistics}");
```

### Pause and Resume

```csharp
var timer = new ChronolapTimer();
timer.Start();

Thread.Sleep(100);
timer.Lap("Before pause");

timer.Pause();
// Timer is paused, elapsed time won't increase
Thread.Sleep(1000); // This won't be counted

timer.Resume();
Thread.Sleep(200);
timer.Lap("After resume");

timer.Stop();
```

### Thread-Safe Usage

Chronolap is fully thread-safe and can be safely used in multi-threaded environments:

```csharp
var timer = new ChronolapTimer();
timer.Start();

// Multiple threads can safely add laps concurrently
Parallel.For(0, 100, i =>
{
    Thread.Sleep(10);
    timer.Lap($"Lap{i}");
});

// Statistics can be calculated while other threads are adding laps
var mean = timer.CalculateLapStatistic(LapStatisticsType.ArithmeticMean);
var fastest = timer.GetFastestLap();

timer.Stop();
```

All public methods and properties are thread-safe, ensuring safe concurrent access from multiple threads.


## OpenTelemetry Activity Extensions Usage

You can easily integrate **Chronolap** lap timings with OpenTelemetry `Activity` by using the provided extension methods. These allow you to add lap duration tags directly to your active tracing activities.

### Example

```csharp
using System.Diagnostics;
using Chronolap;
using Chronolap.OpenTelemetry;

var activitySource = new ActivitySource("MyCompany.MyProduct.MyLibrary");
var timer = new ChronolapTimer();

using (var activity = activitySource.StartActivity("ExampleOperation"))
{
    timer.Start();

    // Perform some operations
    // ...

    timer.Lap("Initialization");
    // Lap after initialization

    timer.Lap("Processing");
    // Lap after processing

    timer.Stop();

    // Add last lap duration as tag
    activity.Lap("Processing", timer);

    // Or export all lap durations as tags at once
    activity.ExportAllLaps(timer);
}
```


## Contributing

Contributions are welcome! Please open issues or pull requests.


## What's New

### v1.3.0 - Thread-Safe Support

**Thread Safety:**
- Full thread-safe implementation using lock mechanism
- Safe concurrent access from multiple threads
- All public methods and properties are thread-safe
- Thread-safe lap recording, statistics calculation, and state management

### v1.2.0 - Advanced Statistics, Performance Improvements & New Features

**New Statistics Features:**
- Min, Max, Variance statistics support
- Percentile calculation (P50, P95, P99, etc.)
- GetFastestLap() and GetSlowestLap() methods
- Configurable minimum lap count for statistics

**Performance Improvements:**
- Cached TotalLapTime calculation (O(1) instead of O(n))
- Configurable maximum lap count

**New Measurement Features:**
- Return value support in measurement methods
- Exception handling support (measurements recorded even when exceptions occur)
- Both synchronous and asynchronous variants

**Configuration:**
- MaxLapCount constructor parameter
- MinimumLapCountForStatistics property

**ILogger Integration:**
- Chronolap can now log lap results directly through ILogger
- Batch Logging: All laps can be logged at once in a clean format
- Customizable Formatting: Use the default format or provide your own custom formatter
- Log Level Support: Log laps at Debug, Information, Warning, or any other log level

### Example - Logging Extension

```csharp
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var chrono = new ChronolapTimer();
chrono.Start();

Thread.Sleep(500);
chrono.Lap("First operation");

Thread.Sleep(700);
chrono.Lap("Second operation");

chrono.Stop();

logger.LogLaps(chrono, LogLevel.Information);
```

Output will looks like this;

```bash
info: Program[0]
      Chronolap Results:
      Lap 1: 00:00:00.5000000
      Lap 2: 00:00:01.2000000
```

## Support

If you find this project useful, consider supporting me:

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-%23FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/ertugrulkara)
