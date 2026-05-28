using STLCompliance.Shared.Data;



namespace ComplianceCore.Api.Entities;



public sealed class Jurisdiction : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid GoverningBodyId { get; set; }



    public string JurisdictionKey { get; set; } = string.Empty;



    public string Label { get; set; } = string.Empty;



    public string Description { get; set; } = string.Empty;



    public bool IsActive { get; set; } = true;



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public GoverningBody? GoverningBody { get; set; }

}


