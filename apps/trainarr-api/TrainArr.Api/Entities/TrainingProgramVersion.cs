using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingProgramVersion : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingProgramId { get; set; }

    public TrainingProgram TrainingProgram { get; set; } = null!;

    public int VersionNumber { get; set; }

    public string Status { get; set; } = "published";

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; set; }

    public Guid? PublishedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<TrainingProgramVersionDefinition> VersionDefinitions { get; set; } = [];
}
