using Microsoft.Extensions.Options;
using RoutArr.Api.Contracts;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DriverEligibilityService(
    TrainArrQualificationCheckClient trainArrClient,
    StaffArrReadinessClient staffArrClient,
    IOptions<DriverEligibilityOptions> eligibilityOptions,
    IRoutArrAuditService audit)
{
    public const string CheckAction = "driver_eligibility.check";

    public async Task<DriverEligibilityCheckResponse> CheckAsync(
        Guid tenantId,
        Guid? actorUserId,
        string personId,
        string? qualificationKey = null,
        string? rulePackKey = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPersonId = ValidatePersonId(personId);
        if (!Guid.TryParse(normalizedPersonId, out var staffarrPersonGuid))
        {
            throw new StlApiException(
                "driver_eligibility.invalid_person_id",
                "Driver person id must be a valid StaffArr person identifier.",
                400);
        }

        var options = eligibilityOptions.Value;
        var effectiveQualificationKey = NormalizeQualificationKey(qualificationKey ?? options.QualificationKey);
        var effectiveRulePackKey = rulePackKey ?? options.RulePackKey;

        DriverEligibilityTrainArrSummary? trainArrSummary = null;
        if (options.CheckTrainArrQualification && trainArrClient.IsConfigured)
        {
            var trainArrResult = await trainArrClient.CheckAsync(
                tenantId,
                staffarrPersonGuid,
                effectiveQualificationKey,
                effectiveRulePackKey,
                cancellationToken);

            if (trainArrResult is not null)
            {
                trainArrSummary = new DriverEligibilityTrainArrSummary(
                    trainArrResult.Outcome,
                    trainArrResult.ReasonCode,
                    trainArrResult.Message,
                    trainArrResult.QualificationKey);
            }
        }

        DriverEligibilityStaffArrSummary? staffArrSummary = null;
        if (options.CheckStaffArrReadiness && staffArrClient.IsConfigured)
        {
            var staffArrResult = await staffArrClient.GetReadinessAsync(
                tenantId,
                staffarrPersonGuid,
                cancellationToken);

            if (staffArrResult is not null)
            {
                staffArrSummary = new DriverEligibilityStaffArrSummary(
                    staffArrResult.ReadinessStatus,
                    staffArrResult.ReadinessBasis,
                    staffArrResult.Blockers.Count,
                    staffArrResult.Blockers.FirstOrDefault()?.Message);
            }
        }

        var outcome = DriverEligibilityRules.MergeOutcome(
            trainArrSummary?.Outcome,
            staffArrSummary?.ReadinessStatus);

        if (trainArrSummary is null && staffArrSummary is null)
        {
            outcome = DriverEligibilityOutcomes.Warn;
        }

        var (reasonCode, message) = trainArrSummary is null && staffArrSummary is null
            ? ("eligibility_check_unavailable", "Driver eligibility integrations are not configured.")
            : DriverEligibilityRules.BuildMergedReason(outcome, trainArrSummary, staffArrSummary);

        var response = new DriverEligibilityCheckResponse(
            normalizedPersonId,
            outcome,
            reasonCode,
            message,
            DriverEligibilityRules.IsBlockingOutcome(outcome),
            trainArrSummary,
            staffArrSummary);

        if (actorUserId.HasValue)
        {
            await audit.WriteAsync(
                CheckAction,
                tenantId,
                actorUserId.Value,
                "person",
                normalizedPersonId,
                outcome,
                cancellationToken: cancellationToken);
        }

        return response;
    }

    public async Task EnsureDriverEligibleAsync(
        Guid tenantId,
        string personId,
        bool ignoreEligibilityBlocks,
        CancellationToken cancellationToken = default)
    {
        if (ignoreEligibilityBlocks)
        {
            return;
        }

        var eligibility = await CheckAsync(tenantId, actorUserId: null, personId, cancellationToken: cancellationToken);
        if (eligibility.IsBlocking)
        {
            throw new StlApiException(
                "dispatch.driver_eligibility_blocked",
                eligibility.Message,
                409,
                eligibility);
        }
    }

    private static string ValidatePersonId(string personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            throw new StlApiException("trip.driver_required", "Driver person id is required.", 400);
        }

        var trimmed = personId.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "trip.driver_id_too_long",
                "Driver person id must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        if (string.IsNullOrWhiteSpace(qualificationKey))
        {
            throw new StlApiException(
                "driver_eligibility.qualification_key_required",
                "Qualification key is required.",
                400);
        }

        return qualificationKey.Trim();
    }
}
