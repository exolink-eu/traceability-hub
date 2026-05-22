namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class LotPayload
{
    public required string Article { get; init; }
    public required string FarmerId { get; init; }
    public string? FarmId { get; init; }
    public DateOnly HarvestDate { get; init; }
    public double Quantity { get; init; }
    public string? Variety { get; init; }
}
