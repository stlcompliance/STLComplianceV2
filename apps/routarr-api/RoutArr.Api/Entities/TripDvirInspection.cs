using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TripDvirInspection : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string Phase { get; set; } = DvirInspectionPhases.PreTrip;

    public string VehicleRefKey { get; set; } = string.Empty;

    public string Result { get; set; } = DvirInspectionResults.Pass;

    public long? OdometerReading { get; set; }

    public string DefectNotes { get; set; } = string.Empty;

    public string SubmittedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Trip Trip { get; set; } = null!;
}

public static class DvirInspectionPhases
{
    public const string PreTrip = "pre_trip";

    public const string PostTrip = "post_trip";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PreTrip,
        PostTrip,
    };
}

public static class DvirInspectionResults
{
    public const string Pass = "pass";

    public const string Fail = "fail";

    public const string Conditional = "conditional";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pass,
        Fail,
        Conditional,
    };
}
