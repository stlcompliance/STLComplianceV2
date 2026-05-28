using STLCompliance.Shared.Data;



namespace MaintainArr.Api.Entities;



public sealed class AssetMeter : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid AssetId { get; set; }



    public string MeterKey { get; set; } = string.Empty;



    public string Name { get; set; } = string.Empty;



    public string Description { get; set; } = string.Empty;



    public string Unit { get; set; } = string.Empty;



    public decimal BaselineReading { get; set; }



    public decimal CurrentReading { get; set; }



    public DateTimeOffset? LastReadingAt { get; set; }



    public string Status { get; set; } = "active";



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }



    public Asset Asset { get; set; } = null!;



    public ICollection<MeterReading> Readings { get; set; } = new List<MeterReading>();

}



public sealed class MeterReading : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid AssetMeterId { get; set; }



    public Guid AssetId { get; set; }



    public decimal ReadingValue { get; set; }



    public decimal DeltaFromPrevious { get; set; }



    public DateTimeOffset ReadAt { get; set; }



    public Guid RecordedByUserId { get; set; }



    public string Notes { get; set; } = string.Empty;



    public bool IsCorrection { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public AssetMeter AssetMeter { get; set; } = null!;



    public Asset Asset { get; set; } = null!;

}



public static class MeterStatuses

{

    public const string Active = "active";

    public const string Inactive = "inactive";

}


