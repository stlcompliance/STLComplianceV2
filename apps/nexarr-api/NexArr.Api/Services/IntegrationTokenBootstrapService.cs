using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace NexArr.Api.Services;

public sealed class IntegrationTokenBootstrapService(
    NexArrDbContext db,
    IConfiguration configuration,
    IOptions<StlServiceTokenOptions> serviceTokenOptions,
    ILogger<IntegrationTokenBootstrapService> logger)
{
    private const int BootstrapLifetimeDays = 365;
    private static readonly ConcurrentDictionary<string, string> ProvisionedAccessTokens = new(StringComparer.Ordinal);

    private static readonly (string Key, string Name, int Order)[] BootstrapProducts =
    [
        ("shared-worker", "STL Shared Worker", 5),
        ("nexarr", "NexArr", 10),
        ("nexarr-worker", "NexArr Worker", 8),
        ("staffarr", "StaffArr", 20),
        ("trainarr", "TrainArr", 30),
        ("maintainarr", "MaintainArr", 40),
        ("routarr", "RoutArr", 50),
        ("supplyarr", "SupplyArr", 60),
        ("customarr", "CustomArr", 55),
        ("ordarr", "OrdArr", 57),
        ("compliancecore", "Compliance Core", 70),
        ("loadarr", "LoadArr", 75),
        ("recordarr", "RecordArr", 76),
        ("assurarr", "AssurArr", 77),
        ("reportarr", "ReportArr", 78),
        ("ledgarr", "LedgArr", 79),
        ("fieldcompanion", "Field Companion", 80),
    ];

    public async Task EnsureProvisionedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureProductCatalogAsync(cancellationToken);

        foreach (var profile in StlIntegrationTokenCatalog.All)
        {
            await EnsureProfileProvisionedAsync(profile, cancellationToken);
        }

        logger.LogInformation(
            "Integration token bootstrap ensured {Count} profile(s).",
            StlIntegrationTokenCatalog.All.Count);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetTokensForConsumerAsync(
        string consumerService,
        CancellationToken cancellationToken = default)
    {
        await EnsureProvisionedAsync(cancellationToken);

        var tokens = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var profile in StlIntegrationTokenCatalog.ForConsumer(consumerService))
        {
            if (ProvisionedAccessTokens.TryGetValue(profile.ProfileKey, out var accessToken))
            {
                tokens[profile.ConfigurationKey] = accessToken;
            }
        }

        return tokens;
    }

    private async Task EnsureProductCatalogAsync(CancellationToken cancellationToken)
    {
        if (await db.ProductCatalog.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var product in BootstrapProducts)
        {
            db.ProductCatalog.Add(new ProductCatalogItem
            {
                ProductKey = product.Key,
                DisplayName = product.Name,
                SortOrder = product.Order,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} product catalog entries for integration bootstrap.", BootstrapProducts.Length);
    }

    private async Task EnsureProfileProvisionedAsync(
        StlIntegrationTokenProfile profile,
        CancellationToken cancellationToken)
    {
        var clientKey = BuildClientKey(profile.ProfileKey);
        if (ProvisionedAccessTokens.TryGetValue(profile.ProfileKey, out var cached)
            && StlIntegrationTokenProvisioner.IsLikelyServiceTokenJwt(cached)
            && await db.ServiceClients.AnyAsync(c => c.ClientKey == clientKey, cancellationToken))
        {
            return;
        }

        await EnsureSourceProductCatalogEntryAsync(profile.SourceProductKey, cancellationToken);
        var client = await db.ServiceClients
            .FirstOrDefaultAsync(c => c.ClientKey == clientKey, cancellationToken);

        if (client is null)
        {
            var now = DateTimeOffset.UtcNow;
            client = new ServiceClient
            {
                Id = Guid.NewGuid(),
                ClientKey = clientKey,
                DisplayName = $"Bootstrap {profile.ProfileKey}",
                SourceProductKey = profile.SourceProductKey,
                AllowedProductKeys = string.Join(',', NormalizeProductKeys(profile.AllowedProductKeys)),
                IsActive = true,
                CreatedAt = now,
                ModifiedAt = now
            };
            db.ServiceClients.Add(client);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueClientKeyViolation(ex))
            {
                db.Entry(client).State = EntityState.Detached;
                client = await db.ServiceClients
                    .FirstAsync(c => c.ClientKey == clientKey, cancellationToken);
            }
        }

        var refreshThreshold = DateTimeOffset.UtcNow.AddDays(30);
        var existingTokens = await db.ServiceTokens
            .AsNoTracking()
            .Where(t => t.ServiceClientId == client.Id
                && t.ActionScope == profile.ActionScope
                && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var existingToken = existingTokens.FirstOrDefault(t => t.ExpiresAt > refreshThreshold);

        if (existingToken is not null
            && ProvisionedAccessTokens.TryGetValue(profile.ProfileKey, out _))
        {
            return;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddDays(BootstrapLifetimeDays);
        var tokenId = Guid.NewGuid();
        var allowedProductKeys = NormalizeProductKeys(profile.AllowedProductKeys);
        var (accessToken, _) = CreateServiceToken(
            client,
            tokenId,
            tenantId: null,
            allowedProductKeys,
            profile.ActionScope,
            expiresAt);

        var record = new ServiceTokenRecord
        {
            Id = tokenId,
            ServiceClientId = client.Id,
            Jti = tokenId.ToString(),
            TokenHash = HashToken(accessToken),
            TenantId = null,
            AllowedProductKeys = string.Join(',', allowedProductKeys),
            ActionScope = profile.ActionScope,
            ExpiresAt = expiresAt,
            IssuedByUserId = Guid.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.ServiceTokens.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        ProvisionedAccessTokens[profile.ProfileKey] = accessToken;

        logger.LogInformation(
            "Issued bootstrap integration token for profile {ProfileKey} (consumer {ConsumerService}).",
            profile.ProfileKey,
            profile.ConsumerService);
    }

    private static string BuildClientKey(string profileKey) => $"bootstrap-{profileKey}";

    private async Task EnsureSourceProductCatalogEntryAsync(
        string sourceProductKey,
        CancellationToken cancellationToken)
    {
        if (await db.ProductCatalog.AnyAsync(product => product.ProductKey == sourceProductKey, cancellationToken))
        {
            return;
        }

        var bootstrapProduct = BootstrapProducts.FirstOrDefault(
            product => string.Equals(product.Key, sourceProductKey, StringComparison.OrdinalIgnoreCase));
        if (bootstrapProduct == default)
        {
            throw new InvalidOperationException(
                $"Bootstrap integration profile references unknown source product '{sourceProductKey}'.");
        }

        db.ProductCatalog.Add(new ProductCatalogItem
        {
            ProductKey = bootstrapProduct.Key,
            DisplayName = bootstrapProduct.Name,
            SortOrder = bootstrapProduct.Order,
            IsActive = true
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private (string Token, DateTimeOffset ExpiresAt) CreateServiceToken(
        ServiceClient client,
        Guid tokenId,
        Guid? tenantId,
        IReadOnlyList<string> allowedProductKeys,
        string? actionScope,
        DateTimeOffset expiresAt)
    {
        var options = serviceTokenOptions.Value;
        var signingKey = configuration["SERVICE_TOKEN_SIGNING_KEY"]
            ?? configuration[$"{StlServiceTokenOptions.SectionName}:SigningKey"]
            ?? configuration["AUTH_SIGNING_KEY"]
            ?? options.SigningKey;

        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            throw new InvalidOperationException("Service token signing key is not configured for integration bootstrap.");
        }

        var issuer = configuration["SERVICE_TOKEN_ISSUER"]
            ?? configuration[$"{StlServiceTokenOptions.SectionName}:Issuer"]
            ?? options.Issuer;
        var audience = configuration["SERVICE_TOKEN_AUDIENCE"]
            ?? configuration[$"{StlServiceTokenOptions.SectionName}:Audience"]
            ?? options.Audience;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
            new(StlServiceTokenClaimTypes.ServiceClientId, client.Id.ToString()),
            new(StlServiceTokenClaimTypes.SourceProduct, client.SourceProductKey),
            new(StlServiceTokenClaimTypes.AllowedProducts, string.Join(',', allowedProductKeys)),
            new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString())
        };

        if (tenantId is Guid scopedTenantId)
        {
            claims.Add(new Claim(StlServiceTokenClaimTypes.TenantScope, scopedTenantId.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(actionScope))
        {
            claims.Add(new Claim(StlServiceTokenClaimTypes.ActionScope, actionScope));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static IReadOnlyList<string> NormalizeProductKeys(IReadOnlyList<string> keys) =>
        keys.Select(k => k.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal).OrderBy(k => k).ToList();

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static bool IsUniqueClientKeyViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(postgresException.ConstraintName, "IX_service_clients_ClientKey", StringComparison.Ordinal);
}
