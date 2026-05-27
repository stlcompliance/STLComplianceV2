using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class ServiceTokenEndpoints
{
    public static void MapServiceTokenEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/service-tokens").WithTags("ServiceTokens").RequireAuthorization();

        group.MapGet("/clients", async (
            HttpContext context,
            ServiceTokenAdminService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListClientsAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListServiceClients");

        group.MapPost("/clients", async (
            RegisterServiceClientRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.RegisterClientAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/service-tokens/clients/{created.ServiceClientId}", created);
        })
        .WithName("RegisterServiceClient");

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

        group.MapPost("/", async (
            IssueServiceTokenRequest request,
            HttpContext context,
            ServiceTokenAdminService service,
            CancellationToken cancellationToken) =>
        {
            var issued = await service.IssueAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/service-tokens/{issued.TokenId}", issued);
        })
        .WithName("IssueServiceToken");

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
