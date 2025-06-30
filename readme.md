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


## Support

If you find this project useful, consider supporting me:

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-%23FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/ertugrulkara)
