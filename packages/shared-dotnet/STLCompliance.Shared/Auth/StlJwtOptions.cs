namespace STLCompliance.Shared.Auth;

public sealed class StlJwtOptions
{
    public const string SectionName = "Auth";

    public string Issuer { get; set; } = "stl-compliance-nexarr";

    public string Audience { get; set; } = "stl-compliance-suite";

  /// <summary>Symmetric signing key (min 32 characters). Set via Auth:SigningKey or AUTH_SIGNING_KEY.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}
