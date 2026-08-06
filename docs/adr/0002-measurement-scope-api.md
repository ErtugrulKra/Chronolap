# ADR 0002: Measurement scope API

- Status: Accepted
- Date: 2026-08-06
- Issue: [#5](https://github.com/ErtugrulKra/Chronolap/issues/5)

## Context

Some operations cross statements or APIs and cannot be expressed naturally as
a delegate. A scope must complete predictably and must not depend on mutable
global stopwatch state.

## Decision

`IChronolap.Measure` starts a measurement and returns an
`IMeasurementScope : IDisposable`:

```csharp
IMeasurementScope Measure(
    string name,
    IReadOnlyDictionary<string, object?>? tags = null);
```

Each scope owns its start timestamp and can complete only once. `Dispose`
completes it successfully unless `Fail(Exception)` or `Cancel()` was called.
Repeated completion or disposal is a no-op. An implementation records duration
with a monotonic clock.

Nested measurements inherit the current measurement as parent through an
async-flow-safe context. Disposing a child does not complete its parent. A scope
disposed from a different async continuation still restores context correctly;
concurrent use of the same scope is unsupported except for idempotent completion.

Scope API failures caused by invalid arguments are thrown before measurement
starts. Exporter or observer failures never escape through `Dispose` and never
change application behavior.

## Tag rules

- Names are non-empty, stable identifiers such as `orders.calculate-total`.
- Tag keys are non-empty, ordinal strings; the last duplicate key wins before
  the immutable start-time snapshot is created.
- Supported values are `string`, `bool`, integral/floating numeric primitives,
  `decimal`, `DateTimeOffset`, `TimeSpan`, `Guid`, enums, and `null`.
- Integrations may normalize supported values but must not mutate caller data.
- Secrets, raw URLs, SQL, payloads, user IDs, and unbounded values are forbidden
  by convention and are not captured automatically.
- Implementations enforce configured tag count and value-length limits.

## Consequences

The API fits `using` blocks and nested operations. C# cannot infer an exception
leaving a `using` block, so callers that need automatic failure classification
should use the delegate API; scope callers explicitly call `Fail` or `Cancel`.
