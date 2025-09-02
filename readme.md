# Chronolap

Advanced stopwatch library with lap tracking support for .NET developers.

## Supported Frameworks

[![.NET Core 3.0+](https://img.shields.io/badge/.NET_Core-3.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/3.0)  
[![.NET 5+](https://img.shields.io/badge/.NET-5.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)  
[![.NET 6+](https://img.shields.io/badge/.NET-6.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)  
[![.NET 7+](https://img.shields.io/badge/.NET-7.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)  
[![.NET Standard 2.1](https://img.shields.io/badge/.NET_Standard-2.1-512BD4?logo=dotnet)](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)

## Features

- Lap tracking  
- Measurement time recording  
- Pause / Resume (coming soon)  
- ILogger logging support  
- Synchronous and asynchronous measurement methods  


## Installation

Install via NuGet:

```bash
dotnet add package Chronolap
```


## Usage

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


## 📢 What's New (v1.2.0)
# 🆕 Lap Logger Extension

ILogger Integration: Chronolap can now log lap results directly through ILogger.

Batch Logging: All laps can be logged at once in a clean format.

Customizable Formatting: Use the default format or provide your own custom formatter.

Log Level Support: Log laps at Debug, Information, Warning, or any other log level.

### Example

```csharp
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var chrono = new ChronolapTimer();
chrono.Start();

Thread.Sleep(500);
chrono.Lap();

Thread.Sleep(700);
chrono.Lap();

chrono.Stop();

logger.LogLaps(chrono, LogLevel.Information);
```

Output will looks like this;

```bash
info: Program[0]
      ⏱ Chronolap Results:
      Lap 1: 00:00:00.5000000
      Lap 2: 00:00:01.2000000
```

## Support

If you find this project useful, consider supporting me:

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-%23FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/ertugrulkara)
