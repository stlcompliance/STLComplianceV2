using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class DefectEscalationRulesTests
{
    private static TenantDefectEscalationSettingsSnapshot EnabledSettings() =>
        new(
            IsEnabled: true,
            LowThresholdHours: 168,
            MediumThresholdHours: 72,
            HighThresholdHours: 24,
            CriticalThresholdHours: 8,
            AutoAcknowledgeOnEscalation: true,
            AutoCreateWorkOrderOnEscalation: true,
            BumpSeverityOnRepeatEscalation: true,
            NotifyOnEscalation: true);

    [Fact]
    public void IsDueForEscalation_returns_true_when_stagnation_exceeds_threshold()
    {
        var asOf = DateTimeOffset.UtcNow;
        var defect = new Defect
        {
            Status = DefectStatuses.Open,
            Severity = DefectSeverities.High,
            UpdatedAt = asOf.AddHours(-30),
        };

        Assert.True(DefectEscalationRules.IsDueForEscalation(defect, EnabledSettings(), asOf));
    }

    [Fact]
    public void IsDueForEscalation_returns_false_when_worker_disabled()
    {
        var asOf = DateTimeOffset.UtcNow;
        var defect = new Defect
        {
            Status = DefectStatuses.Open,
            Severity = DefectSeverities.Critical,
            UpdatedAt = asOf.AddDays(-2),
        };

        var settings = EnabledSettings() with { IsEnabled = false };
        Assert.False(DefectEscalationRules.IsDueForEscalation(defect, settings, asOf));
    }

    [Fact]
    public void BumpSeverity_steps_through_levels()
    {
        Assert.Equal(DefectSeverities.Medium, DefectEscalationRules.BumpSeverity(DefectSeverities.Low));
        Assert.Equal(DefectSeverities.High, DefectEscalationRules.BumpSeverity(DefectSeverities.Medium));
        Assert.Equal(DefectSeverities.Critical, DefectEscalationRules.BumpSeverity(DefectSeverities.High));
        Assert.Null(DefectEscalationRules.BumpSeverity(DefectSeverities.Critical));
    }

    [Theory]
    [InlineData(DefectStatuses.Resolved)]
    [InlineData(DefectStatuses.Closed)]
    public void IsEscalatableStatus_rejects_terminal_statuses(string status)
    {
        Assert.False(DefectEscalationRules.IsEscalatableStatus(status));
    }
}
