namespace STLCompliance.Shared.Auth;

public static class ProductKeyAliases
{
    public const string Companion = "companion";
    public const string FieldCompanion = "fieldcompanion";

    public static string Normalize(string productKey)
    {
        var normalized = productKey.Trim().ToLowerInvariant();
        return normalized switch
        {
            FieldCompanion => Companion,
            _ => normalized,
        };
    }
}
