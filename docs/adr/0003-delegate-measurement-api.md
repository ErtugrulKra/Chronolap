# ADR 0003: Delegate measurement API

- Status: Accepted
- Date: 2026-08-06
- Issue: [#5](https://github.com/ErtugrulKra/Chronolap/issues/5)

## Context

Delegate APIs can classify success, failure, and cancellation automatically.
Sync and async variants should share naming and semantics while preserving user
code behavior.

## Decision

`IChronolap` exposes these overload families (tag parameters omitted here for
readability):

```csharp
void Measure(string name, Action operation);
T Measure<T>(string name, Func<T> operation);
Task MeasureAsync(
    string name,
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default);
Task<T> MeasureAsync<T>(
    string name,
    Func<CancellationToken, Task<T>> operation,
    CancellationToken cancellationToken = default);
```

The async delegate receives the same token supplied to `MeasureAsync`. The
engine awaits the delegate before completing the measurement. Convenience
overloads without a token-aware delegate may be added, but these overloads are
canonical.

Every invocation produces at most one result:

- Normal return records `Success` and preserves the exact return value.
- Any exception records `Failure` and is rethrown with its original instance,
  type, message, stack trace, and inner exception unchanged.
- `OperationCanceledException` records `Cancelled` when its token is the
  supplied token, or when the supplied token is already cancellation-requested.
  It is rethrown unchanged. Other `OperationCanceledException` instances are
  failures because they do not prove cancellation of this operation.
- Cancellation is cooperative. Chronolap never suppresses execution or invents
  an `OperationCanceledException` merely because a token was cancelled.
- Observer/exporter failures are isolated and cannot replace the user result or
  exception.

Null delegates, null/empty names, and invalid tags fail before a measurement is
started. Sync and async operations feed the same immutable measurement result
model.

## Consequences

The API is consistent across sync and async code, preserves application
semantics, and gives cancellation a distinct outcome. Token-aware delegates make
the cancellation contract explicit.
