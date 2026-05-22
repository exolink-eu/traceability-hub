using System.Text.Json;

namespace ExoLink.TraceabilityHub.AspNetCore;

/// <summary>
/// Defines the contract a Traceability App Provider (TAP) must implement to participate
/// in the Traceability Hub exchange protocol.
/// </summary>
public interface ITraceabilityHubService
{
    /// <summary>
    /// Called when the Traceability Hub requests lot data from this TAP.
    /// </summary>
    /// <param name="traceabilityId">The national traceability ID of the lot to export.</param>
    /// <param name="metadata">Optional metadata included in the hub request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TraceabilityLotExport> ExportLotAsync(
        string traceabilityId,
        JsonElement? metadata,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called when the Traceability Hub notifies this TAP of a status change on a lot.
    /// </summary>
    /// <param name="traceabilityId">The national traceability ID of the lot.</param>
    /// <param name="status">The new status reported by the hub.</param>
    /// <param name="metadata">Optional metadata included in the hub request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleStatusUpdateAsync(
        string traceabilityId,
        string status,
        JsonElement? metadata,
        CancellationToken cancellationToken);
}
