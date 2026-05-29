using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Auth;

public sealed record StlServiceTokenPublicJwk(
    string Kty,
    string Use,
    string Kid,
    string Alg,
    string N,
    string E);

public static class StlServiceTokenKeyMaterial
{
    public const string RsaAlgorithm = SecurityAlgorithms.RsaSha256;
    public const string HmacAlgorithm = SecurityAlgorithms.HmacSha256;

    public static string ResolveIssuer(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_ISSUER"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:Issuer"]
        ?? options.Issuer;

    public static string ResolveAudience(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_AUDIENCE"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:Audience"]
        ?? options.Audience;

    public static string ResolveSigningKeyId(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_SIGNING_KEY_ID"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:SigningKeyId"]
        ?? options.SigningKeyId;

    public static SigningCredentials CreateSigningCredentials(
        IConfiguration configuration,
        StlServiceTokenOptions options)
    {
        var keyId = ResolveSigningKeyId(configuration, options);
        var privatePem = ResolvePrivateKeyPem(configuration, options);
        if (!string.IsNullOrWhiteSpace(privatePem))
        {
            return new SigningCredentials(CreateRsaKey(privatePem, keyId), RsaAlgorithm);
        }

        var signingKey = ResolveSymmetricSigningKey(configuration, options);
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new StlApiException(
                "service_token.signing_key_missing",
                "Service token signing key is not configured.",
                500);
        }

        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)) { KeyId = keyId },
            HmacAlgorithm);
    }

    public static TokenValidationParameters BuildValidationParameters(
        IConfiguration configuration,
        StlServiceTokenOptions options)
    {
        var issuer = ResolveIssuer(configuration, options);
        var audience = ResolveAudience(configuration, options);
        var keyId = ResolveSigningKeyId(configuration, options);
        var publicPem = ResolvePublicKeyPem(configuration, options);
        var privatePem = ResolvePrivateKeyPem(configuration, options);
        var issuerSigningKey = !string.IsNullOrWhiteSpace(publicPem)
            ? CreateRsaKey(publicPem, keyId)
            : !string.IsNullOrWhiteSpace(privatePem)
                ? CreateRsaKey(privatePem, keyId)
                : CreateSymmetricValidationKey(configuration, options, keyId);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = issuerSigningKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public static StlServiceTokenPublicJwk? CreatePublicJwk(
        IConfiguration configuration,
        StlServiceTokenOptions options)
    {
        var publicPem = ResolvePublicKeyPem(configuration, options);
        var privatePem = ResolvePrivateKeyPem(configuration, options);
        var pem = !string.IsNullOrWhiteSpace(publicPem) ? publicPem : privatePem;
        if (string.IsNullOrWhiteSpace(pem))
        {
            return null;
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(NormalizePem(pem));
        var parameters = rsa.ExportParameters(false);

        return new StlServiceTokenPublicJwk(
            "RSA",
            "sig",
            ResolveSigningKeyId(configuration, options),
            "RS256",
            Base64UrlEncoder.Encode(parameters.Modulus ?? Array.Empty<byte>()),
            Base64UrlEncoder.Encode(parameters.Exponent ?? Array.Empty<byte>()));
    }

    public static bool IsPublicKeyAvailable(IConfiguration configuration, StlServiceTokenOptions options) =>
        CreatePublicJwk(configuration, options) is not null;

    private static SecurityKey CreateSymmetricValidationKey(
        IConfiguration configuration,
        StlServiceTokenOptions options,
        string keyId)
    {
        var signingKey = ResolveSymmetricSigningKey(configuration, options);
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new StlApiException(
                "service_token.signing_key_missing",
                "Service token signing key is not configured.",
                500);
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)) { KeyId = keyId };
    }

    private static SecurityKey CreateRsaKey(string pem, string keyId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(NormalizePem(pem));
        return new RsaSecurityKey(rsa) { KeyId = keyId };
    }

    private static string ResolvePrivateKeyPem(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_RSA_PRIVATE_KEY"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:RsaPrivateKeyPem"]
        ?? options.RsaPrivateKeyPem;

    private static string ResolvePublicKeyPem(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_RSA_PUBLIC_KEY"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:RsaPublicKeyPem"]
        ?? options.RsaPublicKeyPem;

    private static string ResolveSymmetricSigningKey(IConfiguration configuration, StlServiceTokenOptions options) =>
        configuration["SERVICE_TOKEN_SIGNING_KEY"]
        ?? configuration[$"{StlServiceTokenOptions.SectionName}:SigningKey"]
        ?? configuration["AUTH_SIGNING_KEY"]
        ?? options.SigningKey;

    private static ReadOnlySpan<char> NormalizePem(string pem) =>
        pem.Replace("\\n", "\n", StringComparison.Ordinal).AsSpan();
}
