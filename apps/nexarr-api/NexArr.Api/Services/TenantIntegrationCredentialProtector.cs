using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NexArr.Api.Options;

namespace NexArr.Api.Services;

public sealed record ProtectedTenantIntegrationSecret(
    string CipherText,
    string KeyId);

public sealed class TenantIntegrationCredentialProtector(
    IConfiguration configuration,
    IOptions<TenantIntegrationOptions> options)
{
    private const string FormatPrefix = "v1";

    public ProtectedTenantIntegrationSecret Protect(string plaintext)
    {
        var key = ResolveKey();
        var keyId = ComputeKeyId(key);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);

        var payload = string.Join(
            ".",
            FormatPrefix,
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(cipherBytes),
            Convert.ToBase64String(tag));
        return new ProtectedTenantIntegrationSecret(payload, keyId);
    }

    public string Unprotect(string protectedPayload)
    {
        var key = ResolveKey();
        var parts = protectedPayload.Split('.', StringSplitOptions.None);
        if (parts.Length != 4 || !string.Equals(parts[0], FormatPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Tenant integration credential payload is not in a supported format.");
        }

        var nonce = Convert.FromBase64String(parts[1]);
        var cipherBytes = Convert.FromBase64String(parts[2]);
        var tag = Convert.FromBase64String(parts[3]);
        var plaintextBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plaintextBytes);
        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private byte[] ResolveKey()
    {
        var configured =
            configuration["TENANT_INTEGRATION_ENCRYPTION_KEY"]
            ?? configuration[$"{TenantIntegrationOptions.SectionName}:EncryptionKey"]
            ?? options.Value.EncryptionKey
            ?? configuration["SERVICE_TOKEN_SIGNING_KEY"]
            ?? configuration["AUTH_SIGNING_KEY"]
            ?? configuration["Auth:SigningKey"];

        if (string.IsNullOrWhiteSpace(configured) || configured.Length < 32)
        {
            throw new InvalidOperationException(
                "Tenant integration encryption key must be configured with at least 32 characters.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(configured));
    }

    private static string ComputeKeyId(byte[] key)
    {
        var hash = SHA256.HashData(key);
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}
