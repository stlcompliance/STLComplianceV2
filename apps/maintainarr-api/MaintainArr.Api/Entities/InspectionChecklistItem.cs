using STLCompliance.Shared.Data;



namespace MaintainArr.Api.Entities;



public sealed class InspectionChecklistItem : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid InspectionTemplateId { get; set; }



    public Guid? CategoryId { get; set; }



    public string ItemKey { get; set; } = string.Empty;



    public string Prompt { get; set; } = string.Empty;



    public string ItemType { get; set; } = InspectionChecklistItemTypes.PassFail;



    public bool IsRequired { get; set; } = true;



    public int SortOrder { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public InspectionTemplate InspectionTemplate { get; set; } = null!;



    public InspectionTemplateCategory? Category { get; set; }

}


