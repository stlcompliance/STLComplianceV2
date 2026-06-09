using MaintainArr.Api.Contracts;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;

namespace MaintainArr.Api.Services.Recalls;

public sealed class ManualRecallProvider(IOptions<RecallOptions> options) : RecallProviderBase
{
    private readonly RecallOptions _options = options.Value;

    public override string ProviderKey => "manual";

    public override string DisplayName => "Manual recall";

    public override string SourceOfTruth => "Tenant / manual entry";

    public override bool Enabled => _options.Enabled;

    public override Task<RecallProviderHealthResponse> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var checkedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(new RecallProviderHealthResponse(
            ProviderKey,
            Enabled ? "healthy" : "disabled",
            Enabled ? "Manual recall entry is available." : "Recall features are disabled.",
            checkedAt,
            null));
    }

    public override Task<IReadOnlyList<RecallCampaignSeed>> FindCampaignsByVehicleAsync(
        Guid tenantId,
        int modelYear,
        string make,
        string model,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RecallCampaignSeed>>([]);

    public override Task<RecallCampaignSeed?> FindCampaignByCampaignNumberAsync(
        Guid tenantId,
        string campaignNumber,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<RecallCampaignSeed?>(null);
}
