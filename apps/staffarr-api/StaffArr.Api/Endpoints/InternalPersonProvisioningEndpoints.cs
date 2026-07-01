using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class InternalPersonProvisioningEndpoints
{
    public const string ProvisionActionScope = "staffarr.people.provision";

    public static void MapStaffArrInternalPersonProvisioningEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/people")
            .WithTags("Internal");

        internalApi.MapPost("/provision", async (
            ProvisionStaffArrPersonRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonProvisioningService provisioningService,
            IStaffArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);

            var result = await provisioningService.EnsureProvisionedAsync(
                request.TenantId,
                request.ExternalUserId,
                request.Email,
                request.DisplayName,
                cancellationToken);

            await audit.WriteAsync(
                result.WasCreated ? "people.integration_provisioned" : "people.integration_synced",
                request.TenantId,
                request.RequestedByUserId,
                "person",
                result.Person.Id.ToString(),
                "success",
                reasonCode: "nexarr",
                cancellationToken: cancellationToken);

            return Results.Ok(new ProvisionStaffArrPersonResponse(
                result.Person.Id,
                request.TenantId,
                request.ExternalUserId,
                result.WasCreated,
                result.WasUpdated));
        })
        .WithName("InternalProvisionStaffArrPerson");
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "nexarr",
                RequiredTargetProduct = "staffarr",
                TenantId = tenantId,
                RequiredActionScope = ProvisionActionScope
            });
    }
}
