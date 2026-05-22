using ExoLink.TraceabilityHub.Client.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExoLink.TraceabilityHub.Client;

public sealed class TraceabilityHubApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Lists all articles in the Traceability Hub.
    /// </summary>
    public async Task<List<Article>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        List<Article>? result = await httpClient.GetFromJsonAsync<List<Article>>(
            "api/v1/articles",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    /// <summary>
    /// Gets a specific article from the Traceability Hub by its ID.
    /// Returns null when the article is not found (404).
    /// </summary>
    public async Task<Article?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(
            $"api/v1/articles/{id}",
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Article>(JsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets lots assigned to the current client, optionally filtered by recipient.
    /// </summary>
    public async Task<List<FetchLotResponse>> GetAssignedLotsAsync(string? recipient = null, CancellationToken cancellationToken = default)
    {
        string url = recipient is not null
            ? $"api/v1/lots/assigned?recipient={Uri.EscapeDataString(recipient)}"
            : "api/v1/lots/assigned";

        List<FetchLotResponse>? result = await httpClient.GetFromJsonAsync<List<FetchLotResponse>>(
            url,
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    /// <summary>
    /// Fetches a lot from another system through the Traceability Hub by its exchange ID.
    /// </summary>
    public async Task<FetchLotResponse> FetchLotAsync(string exchangeId, CancellationToken cancellationToken = default)
    {
        FetchLotResponse? result = await httpClient.GetFromJsonAsync<FetchLotResponse>(
            $"api/v1/lots/fetch/{Uri.EscapeDataString(exchangeId)}",
            JsonOptions,
            cancellationToken);

        return result!;
    }

    /// <summary>
    /// Notifies the Traceability Hub that a lot has been imported by its exchange ID.
    /// </summary>
    public async Task<ImportLotResponse> NotifyLotImportedAsync(string exchangeId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsync(
            $"api/v1/lots/imported/{Uri.EscapeDataString(exchangeId)}",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        ImportLotResponse? result = await response.Content.ReadFromJsonAsync<ImportLotResponse>(
            JsonOptions,
            cancellationToken);

        return result!;
    }

    /// <summary>
    /// Creates one or more lots in the Traceability Hub.
    /// </summary>
    public async Task<CreateLotResponse[]> CreateLotsAsync(
        IEnumerable<CreateLotRequest> requests,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "api/v1/lots/create",
            requests,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreateLotResponse[]? result = await response.Content.ReadFromJsonAsync<CreateLotResponse[]>(
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    /// <summary>
    /// Deletes a lot in the Traceability Hub by its lot ID.
    /// Returns false when the lot is not found (404).
    /// </summary>
    public async Task<bool> DeleteLotAsync(string lotId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(
            $"api/v1/lots/{Uri.EscapeDataString(lotId)}",
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }

    /// <summary>
    /// Updates the webhook endpoints of the current client.
    /// Returns false when the client is not found (404).
    /// </summary>
    public async Task<bool> UpdateWebhookEndpointsAsync(UpdateWebhookEndpointsCommand command, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            "api/v1/client/webhooks",
            command,
            JsonOptions,
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
}
