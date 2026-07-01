using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class StaffArrPersonProvisioningClient(
    HttpClient httpClient,
    IOptions<PlatformProductUrlsOptions> productUrls,
    IOptions<StlServiceTokenOptions> serviceTokenOptions,
    IConfiguration configuration) : IStaffArrPersonProvisioningClient
{
    private const string ProvisionActionScope = "staffarr.people.provision";

    public async Task EnsurePersonAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        Guid? requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = StlServiceUrl.NormalizeHttpBaseUrl(productUrls.Value.StaffArrBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "staffarr.provisioning_unavailable",
                "StaffArr provisioning URL is not configured.",
                503);
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/internal/people/provision");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ResolveServiceToken(tenantId));
        request.Content = JsonContent.Create(new
        {
            tenantId,
            externalUserId,
            email,
            displayName,
            requestedByUserId
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new StlApiException(
            "staffarr.provisioning_failed",
            string.IsNullOrWhiteSpace(body)
                ? $"StaffArr person provisioning failed with HTTP {(int)response.StatusCode}."
                : $"StaffArr person provisioning failed with HTTP {(int)response.StatusCode}: {body}",
            (int)response.StatusCode);
    }

    private string ResolveServiceToken(Guid tenantId)
    {
        var configured = configuration["StaffArr:ServiceToken"]
            ?? configuration["StaffArr__ServiceToken"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return CreateScopedProvisioningToken(tenantId);
    }

    private string CreateScopedProvisioningToken(Guid tenantId)
    {
        var options = serviceTokenOptions.Value;
        var issuer = StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options);
        var audience = StlServiceTokenKeyMaterial.ResolveAudience(configuration, options);
        var credentials = StlServiceTokenKeyMaterial.CreateSigningCredentials(configuration, options);
        var tokenId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
            new(StlServiceTokenClaimTypes.ServiceClientId, Guid.Empty.ToString()),
            new(StlServiceTokenClaimTypes.SourceProduct, "nexarr"),
            new(StlServiceTokenClaimTypes.AllowedProducts, "staffarr"),
            new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TenantScope, tenantId.ToString()),
            new(StlServiceTokenClaimTypes.ActionScope, ProvisionActionScope)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
