using STLCompliance.Shared.Data;



namespace MaintainArr.Api.Entities;



public sealed class InspectionTemplateAssetType : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid InspectionTemplateId { get; set; }



    public Guid AssetTypeId { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public InspectionTemplate InspectionTemplate { get; set; } = null!;



    public AssetType AssetType { get; set; } = null!;

}


