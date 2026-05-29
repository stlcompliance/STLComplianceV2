using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class ServiceTokenDiscoveryEndpoints
{
    public static void MapServiceTokenDiscoveryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/.well-known/jwks.json", (ServiceTokenDiscoveryService service) =>
            Results.Ok(service.GetJwks()))
            .AllowAnonymous()
            .WithTags("ServiceTokenDiscovery")
            .WithName("GetServiceTokenJwks");

        app.MapGet("/.well-known/jwks.json", (ServiceTokenDiscoveryService service) =>
            Results.Ok(service.GetJwks()))
            .AllowAnonymous()
            .WithTags("ServiceTokenDiscovery")
            .WithName("GetRootServiceTokenJwks");

        app.MapGet("/api/v1/.well-known/service-token-configuration", (
            HttpRequest request,
            ServiceTokenDiscoveryService service) => Results.Ok(service.GetDiscovery(request)))
            .AllowAnonymous()
            .WithTags("ServiceTokenDiscovery")
            .WithName("GetServiceTokenDiscovery");
    }
}
