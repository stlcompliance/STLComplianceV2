using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingAssignmentLaborEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public TrainingAssignment TrainingAssignment { get; set; } = null!;

    public string LaborTypeKey { get; set; } = string.Empty;

    public decimal HoursWorked { get; set; }

    public decimal CostPerHour { get; set; }

    public string? Notes { get; set; }

    public Guid? LoggedByUserId { get; set; }

    public DateTimeOffset LoggedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
