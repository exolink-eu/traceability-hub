using ExoLink.TraceabilityHub.Client.Models;

namespace ExoLink.TraceabilityHub.AspNetCore;

/// <summary>
/// Represents the lot data exported by a TAP in response to a Traceability Hub fetch request.
/// </summary>
public sealed class TraceabilityLotExport
{
    /// <summary>
    /// An error code when the lot cannot be exported, or <c>null</c> on success.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The individual lot transactions. <c>null</c> when <see cref="ErrorCode"/> is set.
    /// </summary>
    public LotPayload[]? Transactions { get; init; }
}
