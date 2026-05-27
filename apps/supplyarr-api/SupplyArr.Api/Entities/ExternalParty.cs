using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class ExternalParty : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PartyKey { get; set; } = string.Empty;

    public string PartyType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string? TaxIdentifier { get; set; }

    public string ApprovalStatus { get; set; } = "pending";

    public string Status { get; set; } = "active";

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<PartyContact> Contacts { get; set; } = new List<PartyContact>();
}
