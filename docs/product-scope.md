# Chronolap product scope

Chronolap is a small, vendor-neutral measurement and instrumentation toolkit for
.NET applications. It measures named application operations and makes their
results available to logging, metrics, and tracing integrations.

The primary users are ASP.NET Core and Worker Service developers, library
authors, and teams that need production timing data or use OpenTelemetry.

## In scope

- Explicit synchronous and asynchronous operation measurement.
- A disposable scope for code that cannot conveniently be passed as a delegate.
- Duration, outcome, exception type, cancellation, tags, and parent/child
  relationships in one result model.
- Bounded in-memory result storage and optional telemetry exporters.
- A monotonic clock for durations.
- Dependency-injection registration with one main interface, `IChronolap`.

## Out of scope

- A replacement for profilers, application performance monitoring backends,
  distributed tracing SDKs, logging frameworks, or benchmark tools.
- Automatic capture of method arguments, return values, request bodies, SQL,
  URLs, or other sensitive/high-cardinality data.
- Wall-clock timestamps as the source of duration calculations.
- A shared, manually started stopwatch as a requirement for measurement.
- Exporter-specific concepts in the core API.

## Terminology

- **Measurement**: one named operation from start to completion.
- **Measurement scope**: an `IDisposable` handle that completes a measurement.
- **Measurement result**: the immutable completed record.
- **Tag**: caller-supplied metadata attached to a measurement.
- **Outcome**: `Success`, `Failure`, or `Cancelled`.

The accepted public API decisions are recorded in [the ADR index](adr/README.md).
Examples are collected in [measurement API examples](measurement-api-examples.md).
