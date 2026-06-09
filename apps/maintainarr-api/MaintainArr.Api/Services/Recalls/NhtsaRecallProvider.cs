using System.Globalization;
using System.Text;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Options;
using MaintainArr.Api.Services.ExternalIntelligence;
using Microsoft.Extensions.Options;

namespace MaintainArr.Api.Services.Recalls;

public sealed class NhtsaRecallProvider(
    NhtsaExternalIntelligenceClient nhtsaClient,
    IOptions<RecallOptions> options)
    : RecallProviderBase
{
    private readonly RecallOptions _options = options.Value;

    public override string ProviderKey => "nhtsa";

    public override string DisplayName => "NHTSA";

    public override string SourceOfTruth => "NHTSA";

    public override bool Enabled => _options.Enabled && _options.NhtsaEnabled;

    public override Task<RecallProviderHealthResponse> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var checkedAt = DateTimeOffset.UtcNow;
        if (!Enabled)
        {
            return Task.FromResult(new RecallProviderHealthResponse(
                ProviderKey,
                "disabled",
                "NHTSA recall provider is disabled in configuration.",
                checkedAt,
                null));
        }

        return Task.FromResult(new RecallProviderHealthResponse(
            ProviderKey,
            "healthy",
            "NHTSA recall provider is configured and ready.",
            checkedAt,
            null));
    }

    public override async Task<IReadOnlyList<RecallCampaignSeed>> FindCampaignsByVehicleAsync(
        Guid tenantId,
        int modelYear,
        string make,
        string model,
        CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            return [];
        }

        var envelope = await nhtsaClient.GetRecallsByVehicleAsync(tenantId, make, model, modelYear, cancellationToken);
        return envelope.Results
            .Where(result => !string.IsNullOrWhiteSpace(result.NHTSACampaignNumber))
            .GroupBy(result => result.NHTSACampaignNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => ToCampaignSeed(group.First(), modelYear, make, model, "year_make_model"))
            .ToList();
    }

    public override async Task<RecallCampaignSeed?> FindCampaignByCampaignNumberAsync(
        Guid tenantId,
        string campaignNumber,
        CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            return null;
        }

        var envelope = await nhtsaClient.GetRecallsByCampaignNumberAsync(tenantId, campaignNumber, cancellationToken);
        var result = envelope.Results.FirstOrDefault();
        return result is null
            ? null
            : ToCampaignSeed(result, null, null, null, "campaign_number");
    }

    private RecallCampaignSeed ToCampaignSeed(
        NhtsaRecallResult result,
        int? modelYear,
        string? make,
        string? model,
        string matchBasis)
    {
        var normalizedCampaign = result.NHTSACampaignNumber?.Trim();
        var sourceRawJson = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        var potentialUnitsAffected = result.PotentialNumberofUnitsAffected;

        return new RecallCampaignSeed(
            ProviderKey,
            "nhtsa",
            normalizedCampaign,
            normalizedCampaign,
            result.NHTSAActionNumber?.Trim(),
            null,
            normalizedCampaign,
            result.Manufacturer?.Trim() ?? string.Empty,
            result.Component?.Trim() ?? string.Empty,
            NormalizeDate(result.ReportReceivedDate),
            null,
            null,
            "active",
            potentialUnitsAffected,
            result.Summary?.Trim() ?? string.Empty,
            result.Consequence?.Trim() ?? string.Empty,
            result.Remedy?.Trim() ?? string.Empty,
            result.Notes?.Trim() ?? string.Empty,
            result.ParkIt,
            result.ParkOutSide,
            result.OverTheAirUpdate,
            "vehicle",
            $"{nhtsaClient.SafetyApiBaseUrl.TrimEnd('/')}/recalls/campaignNumber?campaignNumber={Uri.EscapeDataString(normalizedCampaign ?? string.Empty)}",
            DateTimeOffset.UtcNow,
            sourceRawJson,
            [
                new RecallCampaignApplicabilityRequest(
                    modelYear,
                    string.IsNullOrWhiteSpace(make) ? result.Make?.Trim() : make.Trim(),
                    string.IsNullOrWhiteSpace(model) ? result.Model?.Trim() : model.Trim(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            ]);
    }

    private static string? NormalizeDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed.ToString("O", CultureInfo.InvariantCulture);
        }

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var local))
        {
            return new DateTimeOffset(local).ToString("O", CultureInfo.InvariantCulture);
        }

        return trimmed;
    }
}
