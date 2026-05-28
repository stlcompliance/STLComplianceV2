using STLCompliance.Shared.Data;



namespace TrainArr.Api.Entities;



public sealed class TrainingProgram : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public string ProgramKey { get; set; } = string.Empty;



    public string Name { get; set; } = string.Empty;



    public string Description { get; set; } = string.Empty;



    public string Status { get; set; } = "draft";



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public ICollection<TrainingProgramDefinition> ProgramDefinitions { get; set; } = [];

}


