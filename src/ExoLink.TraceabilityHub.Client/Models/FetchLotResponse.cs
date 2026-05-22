namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class FetchLotResponse
{
    public FetchLotResult Result { get; init; }
    public LotPayload[]? Transactions { get; init; }
    public string? ExchangeId { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? RecipientCode { get; init; }
}
