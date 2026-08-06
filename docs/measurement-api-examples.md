# Measurement API examples

These examples describe the accepted API contract. The implementation is
scheduled for issue #6.

## 1. Measure a code block

```csharp
using var measurement = chronolap.Measure("orders.calculate-total");
CalculateTotal(order);
```

## 2. Measure a synchronous result

```csharp
var quote = chronolap.Measure(
    "shipping.quote",
    () => shipping.GetQuote(order));
```

## 3. Measure asynchronous I/O with cancellation

```csharp
var order = await chronolap.MeasureAsync(
    "orders.load",
    token => repository.LoadAsync(orderId, token),
    cancellationToken);
```

## 4. Add bounded, low-cardinality tags

```csharp
var tags = new Dictionary<string, object?>
{
    ["order.type"] = order.Type,
    ["customer.tier"] = customer.Tier
};

await chronolap.MeasureAsync(
    "orders.submit",
    token => service.SubmitAsync(order, token),
    cancellationToken,
    tags);
```

## 5. Measure nested business operations

```csharp
using var checkout = chronolap.Measure("checkout");

await chronolap.MeasureAsync(
    "checkout.reserve-stock",
    token => inventory.ReserveAsync(cart, token),
    cancellationToken);

await chronolap.MeasureAsync(
    "checkout.charge",
    token => payments.ChargeAsync(cart, token),
    cancellationToken);
```

## 6. Classify an explicitly managed scope failure

```csharp
using var measurement = chronolap.Measure("imports.process-file");
try
{
    ProcessFile(path);
}
catch (Exception exception)
{
    measurement.Fail(exception);
    throw;
}
```
