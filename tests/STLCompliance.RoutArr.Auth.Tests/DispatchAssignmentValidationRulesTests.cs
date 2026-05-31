using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchAssignmentValidationRulesTests
{
    [Fact]
    public void ApplyValidation_sets_missing_external_data_flag_when_reason_codes_are_unavailable()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            Title = "Validation test trip",
            TripNumber = "TR-VAL-1",
        };

        var preview = new DispatchAssignmentPreviewResponse(
            trip.Id,
            "driver",
            CanAssign: true,
            HasBlockingConflicts: false,
            [],
            [],
            [],
            DriverEligibility: new DispatchAssignmentEligibilitySummary(
                "warn",
                "eligibility_check_unavailable",
                "Eligibility integration unavailable.",
                false,
                null,
                null));

        var validated = DispatchAssignmentValidationRules.ApplyValidation(trip, preview);

        Assert.NotNull(validated.ConflictSummary);
        Assert.True(validated.ConflictSummary!.HasMissingExternalData);
        Assert.False(validated.ConflictSummary.HasStaleExternalData);
        Assert.Contains(validated.ValidationMessages!, message =>
            message.Contains("missing or unavailable", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyValidation_sets_stale_external_data_flag_when_reason_codes_are_stale()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            Title = "Validation stale test trip",
            TripNumber = "TR-VAL-2",
        };

        var preview = new DispatchAssignmentPreviewResponse(
            trip.Id,
            "driver",
            CanAssign: true,
            HasBlockingConflicts: false,
            [],
            [],
            [],
            WorkflowGates: new DispatchAssignmentWorkflowGateSummary(
                "warn",
                "workflow_gate_stale_snapshot",
                "Workflow gate data is stale.",
                false,
                []));

        var validated = DispatchAssignmentValidationRules.ApplyValidation(trip, preview);

        Assert.NotNull(validated.ConflictSummary);
        Assert.False(validated.ConflictSummary!.HasMissingExternalData);
        Assert.True(validated.ConflictSummary.HasStaleExternalData);
        Assert.Contains(validated.ValidationMessages!, message =>
            message.Contains("stale", StringComparison.OrdinalIgnoreCase));
    }
}
