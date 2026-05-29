using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantTripExecutionSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool RequirePreTripDvirBeforeStart { get; set; } = true;

    public bool RequirePostTripDvirBeforeComplete { get; set; }

    public bool RequireDeliveryProofBeforeComplete { get; set; }

    public bool RequirePickupProofBeforeStart { get; set; }

    public bool BlockTripStartOnDvirFail { get; set; } = true;

    public bool BlockTripCompleteOnDvirFail { get; set; } = true;

    public bool RequirePickupProofPhotoBeforeStart { get; set; }

    public bool RequireDeliveryProofPhotoBeforeComplete { get; set; }

    public bool RequireDeliverySignatureBeforeComplete { get; set; }

    public bool RequirePreTripDvirPhotoBeforeStart { get; set; }

    public bool RequirePostTripDvirPhotoBeforeComplete { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
