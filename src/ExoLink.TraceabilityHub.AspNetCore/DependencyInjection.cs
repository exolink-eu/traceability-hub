using ExoLink.TraceabilityHub.AspNetCore.Internal;
using ExoLink.TraceabilityHub.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExoLink.TraceabilityHub.AspNetCore;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the <see cref="TraceabilityHubApiClient"/> with OpenID Connect client-credentials
    /// authentication. Call this on projects that only publish or fetch lots via the hub.
    /// </summary>
    public static IServiceCollection AddTraceabilityHub(
        this IServiceCollection services,
        Action<TraceabilityHubOptions> configure)
    {
        services.Configure(configure);
        services.AddMemoryCache();
        services.AddHttpClient("ExoLink.TraceabilityHub.TokenClient");
        services.AddTransient<ClientCredentialsDelegatingHandler>();

        services.AddHttpClient<TraceabilityHubApiClient>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                TraceabilityHubOptions opts = serviceProvider
                    .GetRequiredService<IOptions<TraceabilityHubOptions>>().Value;

                if (opts.HubBaseUrl is not null)
                {
                    client.BaseAddress = new Uri(opts.HubBaseUrl);
                }
            })
            .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();

        return services;
    }

    /// <summary>
    /// Registers the <see cref="TraceabilityHubApiClient"/> with authentication and registers
    /// <typeparamref name="TService"/> as the <see cref="ITraceabilityHubService"/> that handles
    /// incoming lot-fetch and status-update requests from the hub.
    /// </summary>
    public static IServiceCollection AddTraceabilityHub<TService>(
        this IServiceCollection services,
        Action<TraceabilityHubOptions> configure)
        where TService : class, ITraceabilityHubService
    {
        services.AddScoped<ITraceabilityHubService, TService>();
        return services.AddTraceabilityHub(configure);
    }
}
