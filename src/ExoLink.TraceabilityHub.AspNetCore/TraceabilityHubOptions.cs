namespace ExoLink.TraceabilityHub.AspNetCore;

/// <summary>
/// Configuration options for the Traceability Hub integration.
/// </summary>
public sealed class TraceabilityHubOptions
{
    /// <summary>
    /// The OpenID Connect client ID used to authenticate with the Traceability Hub API.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The OpenID Connect client secret used to authenticate with the Traceability Hub API.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// The OpenID Connect authority. Defaults to <c>https://account.exolink.app</c>.
    /// </summary>
    public string Authority { get; set; } = "https://account.exolink.app";

    /// <summary>
    /// The base URL of the Traceability Hub API.
    /// If not set, the <see cref="ExoLink.TraceabilityHub.Client.TraceabilityHubApiClient"/> must
    /// have its base address configured elsewhere (e.g. via <c>IHttpClientFactory</c>).
    /// </summary>
    public string HubBaseUrl { get; set; } = "https://traceability-hub.exolink.app";

    /// <summary>
    /// The path prefix for the endpoints on which this application receives requests from the
    /// Traceability Hub. Defaults to <c>api/traceability-hub</c>.
    /// </summary>
    public string EndpointPath { get; set; } = "api/traceability-hub";

    /// <summary>
    /// The shared secret used to validate HMAC-SHA256 signatures on incoming requests from the
    /// Traceability Hub and to sign outgoing responses.
    /// </summary>
    public required string SharedSecret { get; set; }
}
