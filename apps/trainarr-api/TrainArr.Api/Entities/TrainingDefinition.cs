using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingDefinition : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string DefinitionKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string QualificationKey { get; set; } = string.Empty;

    public string QualificationName { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
