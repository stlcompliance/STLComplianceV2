using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Auth;

public sealed record ValidatedServiceToken(
    Guid TokenId,
    string SourceProductKey,
    Guid? TenantScope,
    IReadOnlyList<string> AllowedProductKeys,
    string? ActionScope);

public sealed class ServiceTokenRequirements
{
    public required string ExpectedSourceProduct { get; init; }

    public required string RequiredTargetProduct { get; init; }

    public required Guid TenantId { get; init; }

    public string? RequiredActionScope { get; init; }
}

public sealed class StlServiceTokenValidator(
    IConfiguration configuration,
    IOptions<StlServiceTokenOptions> serviceTokenOptions)
{
    public ValidatedServiceToken ValidateOrThrow(string? bearerToken, ServiceTokenRequirements requirements)
    {
        var validated = TryValidate(bearerToken);
        if (validated is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(
                validated.SourceProductKey,
                requirements.ExpectedSourceProduct,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for this integration.",
                403);
        }

        if (!validated.AllowedProductKeys.Any(x =>
                string.Equals(x, requirements.RequiredTargetProduct, StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token is not authorized for the target product.",
                403);
        }

        if (validated.TenantScope is Guid tenantScope && tenantScope != requirements.TenantId)
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token tenant scope does not match the request tenant.",
                403);
        }

        if (!string.IsNullOrWhiteSpace(requirements.RequiredActionScope)
            && !ActionScopeMatches(validated.ActionScope, requirements.RequiredActionScope))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token action scope is not authorized for this integration.",
                403);
        }

        return validated;
    }

    public ValidatedServiceToken? TryValidate(string? bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            return null;
        }

        JwtSecurityToken jwt;
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            handler.ValidateToken(bearerToken, BuildValidationParameters(), out var validatedToken);
            jwt = (JwtSecurityToken)validatedToken;
        }
        catch (SecurityTokenException)
        {
            return null;
        }

        var tokenType = jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.TokenType)?.Value;
        if (!string.Equals(tokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue, StringComparison.Ordinal))
        {
            return null;
        }

        var tokenIdValue = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
            ?? jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.TokenId)?.Value;
        if (tokenIdValue is null || !Guid.TryParse(tokenIdValue, out var tokenId))
        {
            return null;
        }

        var sourceProduct = jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.SourceProduct)?.Value;
        if (string.IsNullOrWhiteSpace(sourceProduct))
        {
            return null;
        }

        var allowedProductsValue = jwt.Claims
            .FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.AllowedProducts)?.Value;
        var allowedProducts = string.IsNullOrWhiteSpace(allowedProductsValue)
            ? Array.Empty<string>()
            : allowedProductsValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Guid? tenantScope = null;
        var tenantScopeValue = jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.TenantScope)?.Value;
        if (Guid.TryParse(tenantScopeValue, out var parsedTenantScope))
        {
            tenantScope = parsedTenantScope;
        }

        var actionScope = jwt.Claims.FirstOrDefault(c => c.Type == StlServiceTokenClaimTypes.ActionScope)?.Value;

        return new ValidatedServiceToken(
            tokenId,
            sourceProduct,
            tenantScope,
            allowedProducts,
            actionScope);
    }

    private static bool ActionScopeMatches(string? tokenActionScope, string requiredActionScope)
    {
        if (string.IsNullOrWhiteSpace(tokenActionScope))
        {
            return false;
        }

        if (string.Equals(tokenActionScope, requiredActionScope, StringComparison.Ordinal))
        {
            return true;
        }

        return tokenActionScope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(scope => string.Equals(scope, requiredActionScope, StringComparison.Ordinal));
    }

    private TokenValidationParameters BuildValidationParameters()
    {
        var options = serviceTokenOptions.Value;
        return StlServiceTokenKeyMaterial.BuildValidationParameters(configuration, options);
    }
}

public static class ServiceTokenBearerParser
{
    public static string? ParseAuthorizationHeader(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : null;
    }
}
