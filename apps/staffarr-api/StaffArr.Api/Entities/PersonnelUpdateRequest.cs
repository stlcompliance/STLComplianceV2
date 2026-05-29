using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonnelUpdateRequest : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string RequestType { get; set; } = PersonnelUpdateRequestTypes.PhoneUpdate;

    public string Status { get; set; } = PersonnelUpdateRequestStatuses.Submitted;

    public string FieldKey { get; set; } = string.Empty;

    public string? CurrentValue { get; set; }

    public string RequestedValue { get; set; } = string.Empty;

    public string? Details { get; set; }

    public Guid SubmittedByUserId { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public string? ReviewNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public StaffPerson? Person { get; set; }
}

public static class PersonnelUpdateRequestTypes
{
    public const string PhoneUpdate = "phone_update";

    public const string ContactInfoUpdate = "contact_info_update";

    public const string ProfileCorrection = "profile_correction";

    public const string Other = "other";
}

public static class PersonnelUpdateRequestStatuses
{
    public const string Submitted = "submitted";

    public const string PendingReview = "pending_review";

    public const string Approved = "approved";

    public const string Denied = "denied";

    public const string Cancelled = "cancelled";
}
