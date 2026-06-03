using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using Xunit;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class ProcurementExceptionEscalationRulesTests
{
    [Fact]
    public void IsDueForEscalation_when_sla_breached_and_never_escalated()
    {
        var entity = new ProcurementException
        {
            Status = ProcurementExceptionStatuses.Open,
            SlaDueAt = DateTimeOffset.UtcNow.AddHours(-2),
            EscalationCount = 0,
            LastEscalatedAt = null,
        };
        var settings = new TenantProcurementExceptionEscalationSettingsSnapshot(
            IsEnabled: true,
            EscalationCooldownHours: 24,
            MaxEscalationsPerException: 5,
            NotifyOnProcurementExceptionSlaEscalation: true,
            AutoCloseCompletedExceptionsEnabled: false,
            AutoCloseCompletedExceptionsAfterHours: ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours);

        Assert.True(ProcurementExceptionEscalationRules.IsDueForEscalation(
            entity,
            settings,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsDueForEscalation_false_when_max_escalations_reached()
    {
        var entity = new ProcurementException
        {
            Status = ProcurementExceptionStatuses.Investigating,
            SlaDueAt = DateTimeOffset.UtcNow.AddHours(-2),
            EscalationCount = 5,
            LastEscalatedAt = DateTimeOffset.UtcNow.AddHours(-48),
        };
        var settings = new TenantProcurementExceptionEscalationSettingsSnapshot(
            IsEnabled: true,
            EscalationCooldownHours: 24,
            MaxEscalationsPerException: 5,
            NotifyOnProcurementExceptionSlaEscalation: true,
            AutoCloseCompletedExceptionsEnabled: false,
            AutoCloseCompletedExceptionsAfterHours: ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours);

        Assert.False(ProcurementExceptionEscalationRules.IsDueForEscalation(
            entity,
            settings,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsDueForEscalation_respects_cooldown_after_previous_escalation()
    {
        var asOf = DateTimeOffset.UtcNow;
        var entity = new ProcurementException
        {
            Status = ProcurementExceptionStatuses.Open,
            SlaDueAt = asOf.AddHours(-48),
            EscalationCount = 1,
            LastEscalatedAt = asOf.AddHours(-2),
        };
        var settings = new TenantProcurementExceptionEscalationSettingsSnapshot(
            IsEnabled: true,
            EscalationCooldownHours: 24,
            MaxEscalationsPerException: 5,
            NotifyOnProcurementExceptionSlaEscalation: true,
            AutoCloseCompletedExceptionsEnabled: false,
            AutoCloseCompletedExceptionsAfterHours: ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours);

        Assert.False(ProcurementExceptionEscalationRules.IsDueForEscalation(entity, settings, asOf));
        Assert.True(ProcurementExceptionEscalationRules.IsDueForEscalation(
            entity,
            settings,
            asOf.AddHours(23)));
    }

    [Fact]
    public void ComputeHoursOverdue_returns_positive_hours_past_sla()
    {
        var slaDueAt = DateTimeOffset.UtcNow.AddHours(-6);
        var hours = ProcurementExceptionEscalationRules.ComputeHoursOverdue(slaDueAt, DateTimeOffset.UtcNow);
        Assert.InRange(hours, 5.9, 6.1);
    }
}
