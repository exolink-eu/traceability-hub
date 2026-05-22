using System.Text.Json;

namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class CreateLotRequest
{
    public required string LotId { get; init; }

    public DateTime? Expiration { get; init; }

    public JsonElement? Metadata { get; init; }

    public string? Recipient { get; init; }
}

