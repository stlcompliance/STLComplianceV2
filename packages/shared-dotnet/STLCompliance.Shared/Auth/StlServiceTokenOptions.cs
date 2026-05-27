namespace STLCompliance.Shared.Auth;

public sealed class StlServiceTokenOptions
{
    public const string SectionName = "ServiceToken";

    public string Issuer { get; set; } = "stl-compliance-services";

    public string Audience { get; set; } = "stl-compliance-services";

    public string SigningKey { get; set; } = string.Empty;

    public int DefaultLifetimeMinutes { get; set; } = 60;
}
