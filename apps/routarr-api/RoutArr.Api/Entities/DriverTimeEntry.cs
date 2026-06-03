using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DriverTimeEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Opaque StaffArr person reference.</summary>
    public string PersonId { get; set; } = string.Empty;

    public string EntryType { get; set; } = DriverTimeEntryTypes.OnDuty;

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string EditReason { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class DriverTimeEntryTypes
{
    public const string OnDuty = "on_duty";

    public const string OffDuty = "off_duty";

    public const string Break = "break";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        OnDuty,
        OffDuty,
        Break,
    };
}
