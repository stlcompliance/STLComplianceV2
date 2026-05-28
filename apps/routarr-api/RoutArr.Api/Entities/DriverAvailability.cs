using STLCompliance.Shared.Data;



namespace RoutArr.Api.Entities;



public sealed class DriverAvailability : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    /// <summary>Opaque StaffArr person identifier.</summary>

    public string PersonId { get; set; } = string.Empty;



    public string AvailabilityStatus { get; set; } = DriverAvailabilityStatuses.Unavailable;



    public DateTimeOffset StartsAt { get; set; }



    public DateTimeOffset EndsAt { get; set; }



    public string Reason { get; set; } = string.Empty;



    public string Notes { get; set; } = string.Empty;



    public Guid CreatedByUserId { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }

}



public static class DriverAvailabilityStatuses

{

    public const string Available = "available";



    public const string Unavailable = "unavailable";



    public const string Limited = "limited";



    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)

    {

        Available,

        Unavailable,

        Limited,

    };



    public static readonly IReadOnlySet<string> BlocksAssignment = new HashSet<string>(StringComparer.OrdinalIgnoreCase)

    {

        Unavailable,

        Limited,

    };

}

