using MaintainArr.Api.Entities;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;

namespace MaintainArr.Api.Services.Recalls;

public sealed class RecallReadinessPolicy(IOptions<RecallOptions> options)
{
    private static readonly string[] HazardKeywords =
    [
        "air bag",
        "airbag",
        "steering",
        "brake",
        "fuel system",
        "cng",
        "lpg",
        "electrical fire",
        "fire risk",
        "wheel",
        "tire",
        "battery",
    ];

    private readonly RecallOptions _options = options.Value;

    public string DetermineReadinessImpact(RecallCampaign campaign, AssetRecallCase? caseEntity = null)
    {
        if (caseEntity is not null && RecallHelpers.IsResolvedCaseStatus(caseEntity.Status))
        {
            return RecallReadinessImpacts.NoHold;
        }

        if (caseEntity is not null && string.Equals(caseEntity.Status, RecallCaseStatuses.CompletedClaimed, StringComparison.OrdinalIgnoreCase))
        {
            return RecallReadinessImpacts.Advisory;
        }

        if (caseEntity is not null && RecallHelpers.IsVerifiedOpenStatus(caseEntity.Status))
        {
            if (campaign.ParkIt)
            {
                return RecallReadinessImpacts.DoNotDrive;
            }

            if (campaign.ParkOutside)
            {
                return RecallReadinessImpacts.ParkOutside;
            }

            return RecallReadinessImpacts.RepairRequired;
        }

        if (campaign.ParkIt && _options.ParkItAutoHold)
        {
            return RecallReadinessImpacts.ParkIt;
        }

        if (campaign.ParkOutside && _options.ParkOutsideAutoHold)
        {
            return RecallReadinessImpacts.ParkOutside;
        }

        if (campaign.OverTheAirUpdate)
        {
            return RecallReadinessImpacts.OverTheAirUpdateAvailable;
        }

        if (HasHazardKeywords(campaign))
        {
            return RecallReadinessImpacts.InspectBeforeUse;
        }

        return RecallReadinessImpacts.Advisory;
    }

    public bool ShouldCreateHold(RecallCampaign campaign, AssetRecallCase? caseEntity = null)
    {
        var readinessImpact = DetermineReadinessImpact(campaign, caseEntity);
        return string.Equals(readinessImpact, RecallReadinessImpacts.ParkIt, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.ParkOutside, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.DoNotDrive, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.OutOfService, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.RepairRequired, StringComparison.OrdinalIgnoreCase);
    }

    public string DetermineHoldSeverity(RecallCampaign campaign, AssetRecallCase? caseEntity = null)
    {
        var readinessImpact = DetermineReadinessImpact(campaign, caseEntity);
        return string.Equals(readinessImpact, RecallReadinessImpacts.Advisory, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.InspectBeforeUse, StringComparison.OrdinalIgnoreCase)
            || string.Equals(readinessImpact, RecallReadinessImpacts.OverTheAirUpdateAvailable, StringComparison.OrdinalIgnoreCase)
            ? "medium"
            : "high";
    }

    public string DetermineMatchStatus(RecallCampaignSeed seed)
    {
        if (seed.ParkIt || seed.ParkOutside)
        {
            return RecallCaseStatuses.NeedsVinCheck;
        }

        return RecallCaseStatuses.PotentialMatch;
    }

    public string DetermineMatchConfidence(RecallCampaignSeed seed)
    {
        if (seed.SourceProvider.Equals(RecallSourceTypes.Nhtsa, StringComparison.OrdinalIgnoreCase))
        {
            return RecallMatchConfidenceLevels.High;
        }

        if (seed.SourceType.Equals(RecallSourceTypes.Manual, StringComparison.OrdinalIgnoreCase))
        {
            return RecallMatchConfidenceLevels.Medium;
        }

        return RecallMatchConfidenceLevels.Low;
    }

    private static bool HasHazardKeywords(RecallCampaign campaign)
    {
        var text = string.Join(' ', new[]
        {
            campaign.Component,
            campaign.Summary,
            campaign.Consequence,
            campaign.Remedy,
            campaign.Notes,
        }).ToLowerInvariant();

        return HazardKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
