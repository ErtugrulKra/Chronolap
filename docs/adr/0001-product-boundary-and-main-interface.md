# ADR 0001: Product boundary and main interface

- Status: Accepted
- Date: 2026-08-06
- Issue: [#5](https://github.com/ErtugrulKra/Chronolap/issues/5)

## Context

The existing API centers on a mutable `ChronolapTimer`. Consumers must manage
start/stop state and choose among many measurement method names. The product
needs one entry point that works for scopes and delegates without requiring a
shared timer.

## Decision

The main interface is `IChronolap`. It is safe to register as a singleton and is
the only service a basic user needs. Its public vocabulary is `Measure`,
`MeasureAsync`, measurement scope, measurement result, tag, and outcome.

The core library owns measurement lifecycle and bounded result dispatch. Logging,
OpenTelemetry, profiling, and framework integrations remain optional packages.
The detailed product boundary is documented in
[`docs/product-scope.md`](../product-scope.md).

The basic DI example remains at most five lines:

```csharp
services.AddChronolap();
var chronolap = provider.GetRequiredService<IChronolap>();
using var measurement = chronolap.Measure("orders.calculate-total");
CalculateTotal(order);
```

## Compatibility

`ChronolapTimer` remains public and behavior-compatible throughout the next
minor release. It is marked legacy in documentation when `IChronolap` ships,
but is not immediately marked `[Obsolete]`. The new engine does not implement
its semantics in terms of a shared timer. A future major release may obsolete
or remove it only after usage feedback and a separate compatibility ADR.

## Consequences

- New users learn one interface and do not coordinate timer state.
- Existing users can migrate operation by operation.
- The timer and measurement engine temporarily coexist.
