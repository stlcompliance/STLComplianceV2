using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingProgramContentReference : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingProgramId { get; set; }

    public TrainingProgram TrainingProgram { get; set; } = null!;

    public string ContentType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string ReferenceValue { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? LocaleTag { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
