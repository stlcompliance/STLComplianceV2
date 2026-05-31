using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class ServiceTokenEndpoints
{
    public static void MapServiceTokenEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/service-tokens").WithTags("ServiceTokens").RequireAuthorization();
        var v1Clients = app.MapGroup("/api/v1/service-clients").WithTags("ServiceTokens").RequireAuthorization();

        static async Task<IResult> ListClientsEndpoint(
            HttpContext context,
            ServiceTokenAdminService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var result = await service.ListClientsAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        }

        group.MapGet("/clients", ListClientsEndpoint)
        .WithName("ListServiceClients");

        v1Clients.MapGet("/", ListClientsEndpoint)
        .WithName("ListServiceClientsV1");

        static async Task<IResult> RegisterClientEndpoint(
            RegisterServiceClientRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            string locationPrefix,
            CancellationToken cancellationToken)
        {
            var created = await service.RegisterClientAsync(context.User, request, cancellationToken);
            return Results.Created($"{locationPrefix}/{created.ServiceClientId}", created);
        }

        group.MapPost("/clients", (
            RegisterServiceClientRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
            RegisterClientEndpoint(request, context, service, "/api/service-tokens/clients", cancellationToken))
        .WithName("RegisterServiceClient");

        v1Clients.MapPost("/", (
            RegisterServiceClientRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
            RegisterClientEndpoint(request, context, service, "/api/v1/service-clients", cancellationToken))
        .WithName("RegisterServiceClientV1");

        static async Task<IResult> RotateClientEndpoint(
            Guid serviceClientId,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken)
        {
            await service.RotateClientAsync(context.User, serviceClientId, cancellationToken);
            return Results.NoContent();
        }

        group.MapPost("/clients/{serviceClientId:guid}/rotate", RotateClientEndpoint)
            .WithName("RotateServiceClient");

        v1Clients.MapPost("/{id:guid}/rotate", async (
            Guid id,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            await service.RotateClientAsync(context.User, id, cancellationToken);
            return Results.NoContent();
        })
            .WithName("RotateServiceClientV1");

        static async Task<IResult> RevokeClientEndpoint(
            Guid serviceClientId,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken)
        {
            await service.RevokeClientAsync(context.User, serviceClientId, cancellationToken);
            return Results.NoContent();
        }

        group.MapPost("/clients/{serviceClientId:guid}/revoke", RevokeClientEndpoint)
            .WithName("RevokeServiceClient");

        v1Clients.MapPost("/{id:guid}/revoke", async (
            Guid id,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            await service.RevokeClientAsync(context.User, id, cancellationToken);
            return Results.NoContent();
        })
            .WithName("RevokeServiceClientV1");

        group.MapGet("/", async (
            HttpContext context,
            ServiceTokenAdminService service,
            Guid? tenantId,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListTokensAsync(context.User, tenantId, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListServiceTokens");

        static async Task<IResult> IssueTokenEndpoint(
            IssueServiceTokenRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            string locationPrefix,
            CancellationToken cancellationToken)
        {
            var issued = await service.IssueAsync(context.User, request, cancellationToken);
            return Results.Created($"{locationPrefix}/{issued.TokenId}", issued);
        }

        group.MapPost("/", (
            IssueServiceTokenRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
            IssueTokenEndpoint(request, context, service, "/api/service-tokens", cancellationToken))
        .WithName("IssueServiceToken");

        app.MapPost("/api/v1/service-token", (
            IssueServiceTokenRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
            IssueTokenEndpoint(request, context, service, "/api/v1/service-token", cancellationToken))
            .WithTags("ServiceTokens")
            .RequireAuthorization()
            .WithName("IssueServiceTokenV1");

        group.MapPost("/validate", async (
            ValidateServiceTokenRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ValidateAsync(context.User, request, cancellationToken));
        })
        .WithName("ValidateServiceToken");

        group.MapPost("/{tokenId:guid}/revoke", async (
            Guid tokenId,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            await service.RevokeAsync(context.User, tokenId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("RevokeServiceToken");
    }
}
