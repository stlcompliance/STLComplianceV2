using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class ServiceTokenAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue,
    IOptions<StlServiceTokenOptions> serviceTokenOptions,
    IConfiguration configuration)
{
    public async Task<PagedResult<ServiceClientResponse>> ListClientsAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.ServiceClients.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var clients = await query
            .OrderBy(c => c.ClientKey)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = clients.Select(ToClientResponse).ToList();

        return new PagedResult<ServiceClientResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<ServiceClientResponse> RegisterClientAsync(
        ClaimsPrincipal principal,
        RegisterServiceClientRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var clientKey = request.ClientKey.Trim().ToLowerInvariant();
        if (await db.ServiceClients.AnyAsync(c => c.ClientKey == clientKey, cancellationToken))
        {
            throw new StlApiException("service_client.key_conflict", "A service client with this key already exists.", 409);
        }

        var sourceProductKey = request.SourceProductKey.Trim().ToLowerInvariant();
        var productExists = await db.ProductCatalog.AnyAsync(p => p.ProductKey == sourceProductKey && p.IsActive, cancellationToken);
        if (!productExists)
        {
            throw new StlApiException("product.not_found", "Source product was not found.", 404);
        }

        var allowedKeys = NormalizeProductKeys(request.AllowedProductKeys);
        await ValidateProductKeysExistAsync(allowedKeys, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var client = new ServiceClient
        {
            Id = Guid.NewGuid(),
            ClientKey = clientKey,
            DisplayName = request.DisplayName.Trim(),
            SourceProductKey = sourceProductKey,
            AllowedProductKeys = string.Join(',', allowedKeys),
            IsActive = true,
            CreatedAt = now,
            ModifiedAt = now
        };

        db.ServiceClients.Add(client);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "service_client.register",
            "service_client",
            client.Id.ToString(),
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.ServiceClientCreated,
            "service_client",
            client.Id.ToString(),
            client.CreatedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                TenantId: null,
                ActorPersonId: principal.GetUserId(),
                TargetType: "service_client",
                TargetId: client.Id.ToString(),
                Summary: $"Service client created: {client.ClientKey}",
                Metadata: new Dictionary<string, string>
                {
                    ["serviceClientId"] = client.Id.ToString(),
                    ["serviceClientKey"] = client.ClientKey,
                    ["sourceProductKey"] = client.SourceProductKey,
                    ["allowedProductKeys"] = client.AllowedProductKeys,
                }),
            cancellationToken: cancellationToken);

        return ToClientResponse(client);
    }

    public async Task<PagedResult<ServiceTokenSummaryResponse>> ListTokensAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.ServiceTokens.AsNoTracking().Include(t => t.ServiceClient).AsQueryable();
        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(t => t.TenantId == scopedTenantId);
        }

        var total = await query.CountAsync(cancellationToken);
        var records = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = records.Select(t => new ServiceTokenSummaryResponse(
            t.Id,
            t.ServiceClientId,
            t.ServiceClient.ClientKey,
            t.TenantId,
            ParseProductKeys(t.AllowedProductKeys),
            t.ActionScope,
            t.ExpiresAt,
            t.RevokedAt,
            t.CreatedAt)).ToList();

        return new PagedResult<ServiceTokenSummaryResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<ServiceTokenIssueResponse> IssueAsync(
        ClaimsPrincipal principal,
        IssueServiceTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var client = await db.ServiceClients
            .FirstOrDefaultAsync(c => c.Id == request.ServiceClientId && c.IsActive, cancellationToken)
            ?? throw new StlApiException("service_client.not_found", "Active service client was not found.", 404);

        if (request.TenantId is Guid tenantId)
        {
            var tenantExists = await db.Tenants.AnyAsync(t => t.Id == tenantId && t.Status == TenantStatuses.Active, cancellationToken);
            if (!tenantExists)
            {
                throw new StlApiException("tenant.not_found", "Active tenant was not found.", 404);
            }
        }

        var allowedKeys = request.AllowedProductKeys is { Count: > 0 }
            ? NormalizeProductKeys(request.AllowedProductKeys)
            : ParseProductKeys(client.AllowedProductKeys);

        await ValidateProductKeysExistAsync(allowedKeys, cancellationToken);

        foreach (var productKey in allowedKeys)
        {
            if (request.TenantId is Guid scopedTenantId)
            {
                var entitled = await db.Entitlements.AnyAsync(
                    e => e.TenantId == scopedTenantId
                        && e.ProductKey == productKey
                        && e.Status == EntitlementStatuses.Active,
                    cancellationToken);
                if (!entitled)
                {
                    throw new StlApiException(
                        "entitlement.missing",
                        $"Tenant is not entitled to product '{productKey}'.",
                        403);
                }
            }
        }

        var options = serviceTokenOptions.Value;
        var lifetimeMinutes = request.LifetimeMinutes is > 0 and <= 24 * 60
            ? request.LifetimeMinutes.Value
            : options.DefaultLifetimeMinutes;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);
        var tokenId = Guid.NewGuid();
        var jti = tokenId.ToString();

        var (accessToken, _) = CreateServiceToken(
            client,
            tokenId,
            request.TenantId,
            allowedKeys,
            request.ActionScope,
            expiresAt);

        var record = new ServiceTokenRecord
        {
            Id = tokenId,
            ServiceClientId = client.Id,
            Jti = jti,
            TokenHash = HashToken(accessToken),
            TenantId = request.TenantId,
            AllowedProductKeys = string.Join(',', allowedKeys),
            ActionScope = request.ActionScope,
            ExpiresAt = expiresAt,
            IssuedByUserId = principal.GetUserId(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.ServiceTokens.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "service_token.issue",
            "service_token",
            record.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new ServiceTokenIssueResponse(
            accessToken,
            record.Id,
            expiresAt,
            client.Id,
            request.TenantId,
            allowedKeys,
            request.ActionScope);
    }

    public async Task<ServiceTokenValidationResponse> ValidateAsync(
        ClaimsPrincipal principal,
        ValidateServiceTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        return await ValidateServiceTokenCoreAsync(request.Token, cancellationToken);
    }

    public Task<ServiceTokenValidationResponse> ValidateForHandoffRedeemAsync(
        string? token,
        CancellationToken cancellationToken = default) =>
        ValidateServiceTokenCoreAsync(token, cancellationToken);

    private async Task<ServiceTokenValidationResponse> ValidateServiceTokenCoreAsync(
        string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Invalid("token_missing");
        }

        JwtSecurityToken jwt;
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var parameters = BuildValidationParameters();
            handler.ValidateToken(token, parameters, out var validatedToken);
            jwt = (JwtSecurityToken)validatedToken;
        }
        catch (Exception) when (token is not null)
        {
            return Invalid("token_invalid");
        }

        var tokenType = jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.TokenType)?.Value;
        if (!string.Equals(tokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue, StringComparison.Ordinal))
        {
            return Invalid("token_type_invalid");
        }

        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
            ?? jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.TokenId)?.Value;
        if (jti is null || !Guid.TryParse(jti, out var tokenId))
        {
            return Invalid("token_id_missing");
        }

        var record = await db.ServiceTokens
            .Include(t => t.ServiceClient)
            .FirstOrDefaultAsync(t => t.Id == tokenId || t.Jti == jti, cancellationToken);

        if (record is null)
        {
            return Invalid("token_not_registered");
        }

        if (record.RevokedAt is not null)
        {
            return Invalid("token_revoked");
        }

        if (record.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Invalid("token_expired");
        }

        if (!record.ServiceClient.IsActive)
        {
            return Invalid("client_inactive");
        }

        if (HashToken(token) != record.TokenHash)
        {
            return Invalid("token_hash_mismatch");
        }

        if (record.TenantId is Guid tenantId)
        {
            var tenantActive = await db.Tenants.AnyAsync(
                t => t.Id == tenantId && t.Status == TenantStatuses.Active,
                cancellationToken);
            if (!tenantActive)
            {
                return Invalid("tenant_inactive");
            }
        }

        var allowedProducts = ParseProductKeys(record.AllowedProductKeys);
        if (record.TenantId is Guid scopedTenantId)
        {
            foreach (var productKey in allowedProducts)
            {
                var entitled = await db.Entitlements.AnyAsync(
                    e => e.TenantId == scopedTenantId
                        && e.ProductKey == productKey
                        && e.Status == EntitlementStatuses.Active,
                    cancellationToken);
                if (!entitled)
                {
                    return Invalid("entitlement_revoked");
                }
            }
        }

        return new ServiceTokenValidationResponse(
            true,
            record.Id,
            record.ServiceClientId,
            record.ServiceClient.SourceProductKey,
            record.TenantId,
            allowedProducts,
            record.ActionScope,
            record.ExpiresAt,
            null);
    }

    public async Task RevokeAsync(
        ClaimsPrincipal principal,
        Guid tokenId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var record = await db.ServiceTokens.FirstOrDefaultAsync(t => t.Id == tokenId, cancellationToken)
            ?? throw new StlApiException("service_token.not_found", "Service token was not found.", 404);

        if (record.RevokedAt is null)
        {
            record.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "service_token.revoke",
                "service_token",
                record.Id.ToString(),
                "Success",
                tenantId: record.TenantId,
                actorUserId: principal.GetUserId(),
                cancellationToken: cancellationToken);
        }
    }

    public async Task RotateClientAsync(
        ClaimsPrincipal principal,
        Guid serviceClientId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var client = await db.ServiceClients.FirstOrDefaultAsync(c => c.Id == serviceClientId, cancellationToken)
            ?? throw new StlApiException("service_client.not_found", "Service client was not found.", 404);

        if (!client.IsActive)
        {
            throw new StlApiException("service_client.inactive", "Service client is inactive.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var activeTokens = await db.ServiceTokens
            .Where(t => t.ServiceClientId == client.Id && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        client.ModifiedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "service_client.rotate",
            "service_client",
            client.Id.ToString(),
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.ServiceClientRotated,
            "service_client",
            client.Id.ToString(),
            now.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                TenantId: null,
                ActorPersonId: principal.GetUserId(),
                TargetType: "service_client",
                TargetId: client.Id.ToString(),
                Summary: $"Service client rotated: {client.ClientKey}",
                Metadata: new Dictionary<string, string>
                {
                    ["serviceClientId"] = client.Id.ToString(),
                    ["serviceClientKey"] = client.ClientKey,
                    ["sourceProductKey"] = client.SourceProductKey,
                    ["revokedTokenCount"] = activeTokens.Count.ToString(),
                }),
            cancellationToken: cancellationToken);
    }

    public async Task RevokeClientAsync(
        ClaimsPrincipal principal,
        Guid serviceClientId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var client = await db.ServiceClients.FirstOrDefaultAsync(c => c.Id == serviceClientId, cancellationToken)
            ?? throw new StlApiException("service_client.not_found", "Service client was not found.", 404);

        var now = DateTimeOffset.UtcNow;
        var wasActive = client.IsActive;
        var activeTokens = await db.ServiceTokens
            .Where(t => t.ServiceClientId == client.Id && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        client.IsActive = false;
        client.ModifiedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "service_client.revoke",
            "service_client",
            client.Id.ToString(),
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        if (wasActive)
        {
            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.ServiceClientRevoked,
                "service_client",
                client.Id.ToString(),
                now.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    TenantId: null,
                    ActorPersonId: principal.GetUserId(),
                    TargetType: "service_client",
                    TargetId: client.Id.ToString(),
                    Summary: $"Service client revoked: {client.ClientKey}",
                    Metadata: new Dictionary<string, string>
                    {
                        ["serviceClientId"] = client.Id.ToString(),
                        ["serviceClientKey"] = client.ClientKey,
                        ["sourceProductKey"] = client.SourceProductKey,
                        ["revokedTokenCount"] = activeTokens.Count.ToString(),
                    }),
                cancellationToken: cancellationToken);
        }
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
        var issuer = StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options);
        var audience = StlServiceTokenKeyMaterial.ResolveAudience(configuration, options);

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

        var credentials = StlServiceTokenKeyMaterial.CreateSigningCredentials(configuration, options);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private TokenValidationParameters BuildValidationParameters()
    {
        var options = serviceTokenOptions.Value;
        return StlServiceTokenKeyMaterial.BuildValidationParameters(configuration, options);
    }

    private async Task ValidateProductKeysExistAsync(IReadOnlyList<string> productKeys, CancellationToken cancellationToken)
    {
        foreach (var key in productKeys)
        {
            var exists = await db.ProductCatalog.AnyAsync(p => p.ProductKey == key && p.IsActive, cancellationToken);
            if (!exists)
            {
                throw new StlApiException("product.not_found", $"Product '{key}' was not found.", 404);
            }
        }
    }

    private static ServiceClientResponse ToClientResponse(ServiceClient client) =>
        new(
            client.Id,
            client.ClientKey,
            client.DisplayName,
            client.SourceProductKey,
            ParseProductKeys(client.AllowedProductKeys),
            client.IsActive,
            client.CreatedAt);

    private static IReadOnlyList<string> NormalizeProductKeys(IReadOnlyList<string> keys) =>
        keys.Select(k => k.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal).OrderBy(k => k).ToList();

    private static IReadOnlyList<string> ParseProductKeys(string raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static ServiceTokenValidationResponse Invalid(string reasonCode) =>
        new(false, null, null, null, null, [], null, null, reasonCode);
}
