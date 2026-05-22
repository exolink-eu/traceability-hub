# ExoLink TraceabilityHub Client

Client libraries for integrating with the ExoLink TraceabilityHub — a platform that facilitates the exchange of lot traceability data between Traceability App Providers (TAPs).

Two NuGet packages are available:

| Package | Description |
|---|---|
| `ExoLink.TraceabilityHub.Client` | Typed HTTP client for the TraceabilityHub API. Use this when you only need to call the hub (create lots, fetch lots, etc.). |
| `ExoLink.TraceabilityHub.AspNetCore` | ASP.NET Core integration. Includes DI registration, OIDC client-credentials authentication, and minimal-API endpoints for receiving callbacks from the hub. Depends on `ExoLink.TraceabilityHub.Client`. |

Both packages target **net9.0** and **net10.0**.

---

## ASP.NET Core integration

### Installation

```
dotnet add package ExoLink.TraceabilityHub.AspNetCore
```

### Registration

Call `AddTraceabilityHub` in `Program.cs`. If your application also receives requests from the hub (lot-fetch and status-update callbacks), provide an implementation of `ITraceabilityHubService` as the generic parameter.

```csharp
// Program.cs
builder.Services.AddTraceabilityHub<MyTraceabilityHubService>(options =>
{
    options.ClientId     = builder.Configuration["TraceabilityHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["TraceabilityHub:ClientSecret"]!;
    options.SharedSecret = builder.Configuration["TraceabilityHub:SharedSecret"]!;

    // Optional — defaults shown below:
    // options.Authority    = "https://account.exolink.app";
    // options.HubBaseUrl   = "https://traceability-hub.exolink.app";
    // options.EndpointPath = "api/traceability-hub";
});
```

If you only publish or fetch lots and do not receive callbacks, omit the generic parameter:

```csharp
builder.Services.AddTraceabilityHub(options => { /* ... */ });
```

### Mapping callback endpoints

After building the app, map the hub callback endpoints. The routes are registered under the path configured in `TraceabilityHubOptions.EndpointPath` (default: `api/traceability-hub`).

```csharp
var app = builder.Build();

app.MapTraceabilityHubEndpoints();
```

This registers the following routes:

- `POST api/traceability-hub/fetch` — called by the hub to request lot data from your application.
- `POST api/traceability-hub/status` — called by the hub to notify your application of a lot status change.

Both endpoints validate the HMAC-SHA256 request signature using the configured `SharedSecret`.

### Implementing `ITraceabilityHubService`

```csharp
using System.Text.Json;
using ExoLink.TraceabilityHub.AspNetCore;
using ExoLink.TraceabilityHub.Client.Models;

public class MyTraceabilityHubService : ITraceabilityHubService
{
    public async Task<TraceabilityLotExport> ExportLotAsync(
        string traceabilityId,
        JsonElement? metadata,
        CancellationToken cancellationToken)
    {
        // Look up the lot in your own system by its national traceability ID.
        LotPayload? lot = await myDatabase.FindLotAsync(traceabilityId, cancellationToken);

        if (lot is null)
        {
            return new TraceabilityLotExport { ErrorCode = "NOT_FOUND" };
        }

        return new TraceabilityLotExport
        {
            Transactions = [lot]
        };
    }

    public async Task HandleStatusUpdateAsync(
        string traceabilityId,
        string status,
        JsonElement? metadata,
        CancellationToken cancellationToken)
    {
        // React to a status change reported by the hub.
        await myDatabase.UpdateLotStatusAsync(traceabilityId, status, cancellationToken);
    }
}
```

### Registering webhook endpoints

After your application starts, call `UpdateWebhookEndpointsAsync` once (e.g. on startup or during configuration) to inform the hub where it should send callbacks:

```csharp
public class TraceabilityHubStartupService(TraceabilityHubApiClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.UpdateWebhookEndpointsAsync(new UpdateWebhookEndpointsCommand
        {
            FetchLotUri     = "https://my-app.example.com/api/traceability-hub/fetch",
            StatusUpdateUri = "https://my-app.example.com/api/traceability-hub/status",
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

---

## TraceabilityHubApiClient

`TraceabilityHubApiClient` is the typed HTTP client for the TraceabilityHub REST API. It is registered automatically by `AddTraceabilityHub` and can be injected directly.

### Working with articles

```csharp
// List all articles supported by the hub.
List<Article> articles = await client.GetArticlesAsync();

// Get a specific article by its ID.
Article? article = await client.GetArticleAsync(articleId);
```

### Creating lots

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

### Fetching lots

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

### Retrieving assigned lots

```csharp
// Get all lots assigned to the current client.
List<FetchLotResponse> assignedLots = await client.GetAssignedLotsAsync();

// Optionally filter by recipient code.
List<FetchLotResponse> filtered = await client.GetAssignedLotsAsync(recipient: "RECIPIENT-CODE");
```

### Deleting a lot

```csharp
bool deleted = await client.DeleteLotAsync("MY-LOT-001");
```

---

## Configuration reference

| Property | Required | Default | Description |
|---|---|---|---|
| `ClientId` | Yes | — | OIDC client ID for authenticating with the hub API. |
| `ClientSecret` | Yes | — | OIDC client secret. |
| `SharedSecret` | Yes | — | HMAC-SHA256 secret for signing and verifying hub callbacks. |
| `Authority` | No | `https://account.exolink.app` | OIDC authority URL. |
| `HubBaseUrl` | No | `https://traceability-hub.exolink.app` | Base URL of the TraceabilityHub API. |
| `EndpointPath` | No | `api/traceability-hub` | Path prefix for incoming hub callback endpoints. |

---

## License

Copyright © 2026 ExoLink B.V. — MIT License
