using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ExoLink.TraceabilityHub.AspNetCore.Internal;

internal sealed class ClientCredentialsDelegatingHandler(
    IOptions<TraceabilityHubOptions> options,
    IHttpClientFactory httpClientFactory,
    IMemoryCache memoryCache) : DelegatingHandler
{
    private const string TokenCacheKey = "ExoLink.TraceabilityHub.AccessToken";
    private const string TokenClientName = "ExoLink.TraceabilityHub.TokenClient";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string token = await GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue<string>(TokenCacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        TraceabilityHubOptions opts = options.Value;
        string tokenEndpoint = $"{opts.Authority.TrimEnd('/')}/connect/token";

        HttpClient client = httpClientFactory.CreateClient(TokenClientName);

        FormUrlEncodedContent body = new(
        [
            new KeyValuePair<string, string>("client_id", opts.ClientId),
            new KeyValuePair<string, string>("client_secret", opts.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "exolink:traceability")
        ]);

        HttpResponseMessage response = await client.PostAsync(tokenEndpoint, body, cancellationToken);
        response.EnsureSuccessStatusCode();

        TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(
            cancellationToken: cancellationToken);

        if (tokenResponse?.AccessToken is null)
        {
            throw new InvalidOperationException(
                "The Traceability Hub authority did not return an access token.");
        }

        TimeSpan expiry = TimeSpan.FromSeconds(Math.Max(tokenResponse.ExpiresIn - 30, 30));
        memoryCache.Set(TokenCacheKey, tokenResponse.AccessToken, expiry);

        return tokenResponse.AccessToken;
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
