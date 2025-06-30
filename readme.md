# Chronolap

Advanced stopwatch library with lap tracking support for .NET developers.

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


## Contributing

Contributions are welcome! Please open issues or pull requests.