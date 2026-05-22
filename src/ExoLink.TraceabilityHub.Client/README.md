# ExoLink.TraceabilityHub.Client

Typed HTTP client for the [ExoLink TraceabilityHub](https://traceability-hub.exolink.app) API — a platform that facilitates the exchange of lot traceability data between Traceability App Providers (TAPs).

For full ASP.NET Core integration (DI registration, OIDC authentication, and hub callback endpoints) see the `ExoLink.TraceabilityHub.AspNetCore` package.

Targets **net9.0** and **net10.0**.

---

## Installation

```
dotnet add package ExoLink.TraceabilityHub.Client
```

When using the `ExoLink.TraceabilityHub.AspNetCore` package, `TraceabilityHubApiClient` is registered and configured automatically — no manual setup is needed.

For standalone use, register the client manually and configure its base address and any authentication handler:

```csharp
builder.Services.AddHttpClient<TraceabilityHubApiClient>(client =>
{
    client.BaseAddress = new Uri("https://traceability-hub.exolink.app");
});
```

---

## Working with articles

```csharp
// List all articles supported by the hub.
List<Article> articles = await client.GetArticlesAsync();

// Get a specific article by its ID.
Article? article = await client.GetArticleAsync(articleId);
```

---

## Creating lots

```csharp
CreateLotRequest[] requests =
[
    new CreateLotRequest
    {
        LotId     = "MY-LOT-001",
        Recipient = "RECIPIENT-CODE",
        Expiration = DateTime.UtcNow.AddDays(30),
    },
];

CreateLotResponse[] responses = await client.CreateLotsAsync(requests);

foreach (CreateLotResponse response in responses)
{
    if (response.Status == CreateLotResult.Created)
    {
        Console.WriteLine($"Lot created. TraceabilityId: {response.NationalTraceabilityId}");
    }
    else
    {
        Console.WriteLine($"Failed to create lot '{response.LotId}': {response.Status}");
    }
}
```

---

## Fetching lots

```csharp
// Fetch a specific lot from another TAP using its exchange ID.
FetchLotResponse lot = await client.FetchLotAsync(exchangeId);

if (lot.Result == FetchLotResult.Success)
{
    foreach (LotPayload transaction in lot.Transactions!)
    {
        Console.WriteLine($"Article: {transaction.Article}, Quantity: {transaction.Quantity}");
    }

    // Notify the hub that the lot has been imported.
    await client.NotifyLotImportedAsync(lot.ExchangeId!);
}
```

---

## Retrieving assigned lots

```csharp
// Get all lots assigned to the current client.
List<FetchLotResponse> assignedLots = await client.GetAssignedLotsAsync();

// Optionally filter by recipient code.
List<FetchLotResponse> filtered = await client.GetAssignedLotsAsync(recipient: "RECIPIENT-CODE");
```

---

## Deleting a lot

```csharp
bool deleted = await client.DeleteLotAsync("MY-LOT-001");
```

---

## License

Copyright © 2026 ExoLink B.V. — MIT License
