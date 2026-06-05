namespace STLCompliance.Shared.Auth;

public static class ProductKeyAliases
{
    public static string Normalize(string productKey)
    {
        return productKey.Trim().ToLowerInvariant().Replace("-", "").Replace("_", "");
    }
}
