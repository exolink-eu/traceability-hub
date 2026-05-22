using System.Text.Json;

namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class CreateLotResponse
{
    public required string LotId { get; init; }
    public string? NationalTraceabilityId { get; init; }
    public DateTime? Expiration { get; init; }
    public JsonElement? Metadata { get; init; }
    public CreateLotResult Status { get; init; }
}
