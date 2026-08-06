# Migrating from ChronolapTimer

`ChronolapTimer` remains supported while the measurement API is introduced. New
code should depend on `IChronolap`; existing code can migrate one operation at a
time without sharing a timer.

| `ChronolapTimer` usage | Measurement API |
| --- | --- |
| `Start()` + `Lap(name)` | `using var scope = chronolap.Measure(name)` |
| `MeasureExecutionTime(action, name)` | `chronolap.Measure(name, action)` |
| `MeasureExecutionTime<T>(func, name)` | `chronolap.Measure(name, func)` |
| `MeasureExecutionTimeAsync(func, name)` | `chronolap.MeasureAsync(name, token => func(), token)` |
| `MeasureExecutionTimeWithExceptionHandling(...)` | `Measure`/`MeasureAsync`; failures are always recorded and rethrown |
| `Laps` and timer statistics | Query the bounded result store introduced by issue #6 |

## Rollout

1. Register Chronolap and inject `IChronolap` beside any existing timer.
2. Replace self-contained delegate measurements first.
3. Replace start/lap regions with a separate scope per logical operation.
4. Move statistics and export consumers to measurement results.
5. Remove timer injection only after all consumers have migrated.

There is intentionally no adapter that maps one shared `ChronolapTimer` to
multiple concurrent measurements: its stateful lap semantics cannot preserve
independent operation boundaries safely.
