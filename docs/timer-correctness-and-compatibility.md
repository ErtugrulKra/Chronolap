# Timer correctness and compatibility

ChronolapTimer serializes all timer state access through one synchronization object. `Start`, `Stop`, `Pause`, `Resume`, `Reset`, `Elapsed`, `IsRunning`, `IsPaused`, and `Lap` therefore observe a consistent stopwatch state. A `Lap` operation reads the monotonic stopwatch timestamp, calculates its duration, updates the previous timestamp, and stores the lap in one critical section. Concurrent lap timestamps cannot move backwards and their durations cannot be negative.

`MinimumLapCountForStatistics` must be greater than zero. The constructor and property setter throw `ArgumentOutOfRangeException` for zero or negative values. Both `AddChronolap` configuration overloads apply the same rule during service registration rather than delaying the failure until the timer is resolved.

For a data set containing one lap, sample variance and sample standard deviation are defined as `0`. They never return `NaN`.

## Percentile migration

`LapStatisticsType.Percentile` was removed because `CalculateLapStatistic` cannot accept the percentile value required to implement that enum member. Use the dedicated API instead:

```csharp
var p95 = timer.CalculatePercentile(95);
```

This is a source-breaking change for code that names `LapStatisticsType.Percentile`; such code must switch to `CalculatePercentile`. Enum constants are normally embedded in compiled callers, so an already compiled caller may still pass the old numeric value (`6`). That value is no longer defined and `CalculateLapStatistic` rejects it with `ArgumentOutOfRangeException`. Recompile dependent applications after migration. The numeric values of the remaining enum members (`0` through `5`) are unchanged.