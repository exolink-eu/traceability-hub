using System.Text.Json;

namespace ExoLink.TraceabilityHub.AspNetCore.Internal;

internal sealed record HubFetchRequest(string LotId, string TraceabilityId, JsonElement? Metadata);

internal sealed record HubStatusRequest(
    string LotId,
    string TraceabilityId,
    string Status,
    JsonElement? Metadata);
