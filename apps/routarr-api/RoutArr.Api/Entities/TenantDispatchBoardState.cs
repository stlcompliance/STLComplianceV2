using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantDispatchBoardState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string DefaultScope { get; set; } = DispatchBoardScopes.Daily;

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public static class DispatchBoardScopes
{
    public const string Daily = "daily";

    public const string Weekly = "weekly";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Daily,
        Weekly,
    };
}
