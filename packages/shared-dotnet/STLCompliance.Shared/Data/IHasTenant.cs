namespace STLCompliance.Shared.Data;

public interface IHasTenant
{
    Guid TenantId { get; set; }
}
