using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;

namespace NexArr.Api.Services;

public sealed class MfaSecretProtector(TenantIntegrationCredentialProtector tenantIntegrationCredentialProtector)
{
    public string Protect(string plaintext) =>
        tenantIntegrationCredentialProtector.Protect(plaintext).CipherText;

    public bool IsProtectedPayload(string? payload) =>
        !string.IsNullOrWhiteSpace(payload)
        && tenantIntegrationCredentialProtector.IsProtectedPayload(payload);

    public bool TryResolvePlaintext(string? payload, out string plaintext) =>
        tenantIntegrationCredentialProtector.TryResolvePlaintext(payload, out plaintext);

    public async Task<int> MigrateLegacySecretsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken = default)
    {
        var credentials = await db.UserCredentials
            .Where(credential =>
                credential.IsMfaEnabled
                && credential.MfaSecret != null
                && !IsProtectedPayload(credential.MfaSecret))
            .ToListAsync(cancellationToken);

        if (credentials.Count == 0)
        {
            return 0;
        }

        foreach (var credential in credentials)
        {
            credential.MfaSecret = Protect(credential.MfaSecret!);
        }

        await db.SaveChangesAsync(cancellationToken);
        return credentials.Count;
    }
}
