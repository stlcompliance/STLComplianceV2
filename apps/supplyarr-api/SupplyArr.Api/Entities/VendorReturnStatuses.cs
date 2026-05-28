namespace SupplyArr.Api.Entities;



public static class VendorReturnStatuses

{

    public const string Draft = "draft";



    public const string Posted = "posted";



    public const string Cancelled = "cancelled";



    public static readonly HashSet<string> Editable = new(StringComparer.OrdinalIgnoreCase)

    {

        Draft

    };

}

