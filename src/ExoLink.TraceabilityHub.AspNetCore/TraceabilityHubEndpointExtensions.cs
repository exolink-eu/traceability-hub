using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ExoLink.TraceabilityHub.AspNetCore.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExoLink.TraceabilityHub.AspNetCore;

public static class TraceabilityHubEndpointExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Maps the Traceability Hub callback endpoints under the path configured in
    /// <see cref="TraceabilityHubOptions.EndpointPath"/> and returns the
    /// <see cref="RouteGroupBuilder"/> for further configuration.
    /// </summary>
    public static RouteGroupBuilder MapTraceabilityHubEndpoints(this IEndpointRouteBuilder endpoints)
    {
        string endpointPath = endpoints.ServiceProvider
            .GetRequiredService<IOptions<TraceabilityHubOptions>>().Value.EndpointPath;

        RouteGroupBuilder group = endpoints.MapGroup(endpointPath);

        group.MapPost("/fetch", HandleFetchAsync)
            .Produces<TraceabilityLotExport>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/status", HandleStatusUpdateAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> HandleFetchAsync(
        string hash,
        HttpContext context,
        ITraceabilityHubService service,
        IOptions<TraceabilityHubOptions> options)
    {
        string body = await new StreamReader(context.Request.Body)
            .ReadToEndAsync(context.RequestAborted);

        string secret = options.Value.SharedSecret;

        if (!ValidateHash(body, secret, hash))
        {
            return Results.BadRequest("Invalid request hash.");
        }

        HubFetchRequest? request = JsonSerializer.Deserialize<HubFetchRequest>(body, JsonOptions);

        if (request is null)
        {
            return Results.BadRequest("Invalid request payload.");
        }

        TraceabilityLotExport result = await service.ExportLotAsync(
            request.TraceabilityId, request.Metadata, context.RequestAborted);

        return CreateSignedResponse(context.Response, result, secret);
    }

    private static async Task<IResult> HandleStatusUpdateAsync(
        string hash,
        HttpContext context,
        ITraceabilityHubService service,
        IOptions<TraceabilityHubOptions> options)
    {
        string body = await new StreamReader(context.Request.Body)
            .ReadToEndAsync(context.RequestAborted);

        string secret = options.Value.SharedSecret;

        if (!ValidateHash(body, secret, hash))
        {
            return Results.BadRequest("Invalid request hash.");
        }

        HubStatusRequest? request = JsonSerializer.Deserialize<HubStatusRequest>(body, JsonOptions);

        if (request is null)
        {
            return Results.BadRequest("Invalid request payload.");
        }

        await service.HandleStatusUpdateAsync(
            request.TraceabilityId, request.Status, request.Metadata, context.RequestAborted);

        return Results.Ok();
    }

    private static string ComputeHash(string payload, string secret)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        byte[] hashBytes = HMACSHA256.HashData(secretBytes, payloadBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static bool ValidateHash(string payload, string secret, string providedHash)
    {
        string computed = ComputeHash(payload, secret);
        return computed.Equals(providedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static IResult CreateSignedResponse<T>(HttpResponse response, T payload, string secret)
    {
        string responseBody = JsonSerializer.Serialize(payload, JsonOptions);
        string hash = ComputeHash(responseBody, secret);
        response.Headers["X-TraceabilityHub-Hash-256"] = hash;
        return Results.Content(responseBody, "application/json");
    }
}
