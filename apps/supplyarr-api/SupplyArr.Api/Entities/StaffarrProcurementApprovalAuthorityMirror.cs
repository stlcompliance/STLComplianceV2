using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class StaffarrProcurementApprovalAuthorityMirror : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public Guid ExternalUserId { get; set; }

    public bool CanSubmitPurchaseRequests { get; set; }

    public bool CanApprovePurchaseRequests { get; set; }

    public bool CanIssuePurchaseOrders { get; set; }

    public decimal? MaxSubmitAmount { get; set; }

    public decimal? MaxApproveAmount { get; set; }

    public decimal? MaxIssueAmount { get; set; }

    public string OrgUnitScopeIdsJson { get; set; } = "[]";

    public string GrantsJson { get; set; } = "[]";

    public DateTimeOffset SourceComputedAt { get; set; }

    public DateTimeOffset RefreshedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
