using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;
using TrainArr.Api.Options;

namespace TrainArr.Api.Services;

public sealed class StaffArrSiteReferenceService(
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public async Task ValidateActiveSiteAsync(
        Guid tenantId,
        string scopeKey,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(scopeKey, out var siteId))
        {
            throw new StlApiException(
                "training_applicability.site_scope_invalid",
                "Site applicability scope key must be a StaffArr site org unit id.",
                400);
        }

        var site = await client.GetAsync(
            tenantId,
            siteId,
            options.Value.ServiceToken,
            cancellationToken: cancellationToken);
        if (site is null)
        {
            throw new StlApiException(
                "training_applicability.site_not_found",
                "Active StaffArr site was not found for this applicability profile.",
                404);
        }
    }
}
