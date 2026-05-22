namespace ExoLink.TraceabilityHub.Client.Models;

public sealed class UpdateWebhookEndpointsCommand
{
    public required string FetchLotUri { get; init; }

    public required string StatusUpdateUri { get; init; }
}
